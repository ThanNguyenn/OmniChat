using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Requests.Intent;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.BackgroundJobs
{
    public class ChatAggregationWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ChatAggregationWorker> _logger;
        private readonly IHubContext<SupportConversationHub> _hubContext;

        public ChatAggregationWorker(
            IConnectionMultiplexer redis,
            IServiceScopeFactory scopeFactory,
            ILogger<ChatAggregationWorker> logger,
            IHubContext<SupportConversationHub> hubContext
        )
        {
            _redis = redis;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = _redis.GetDatabase();

            _logger.LogInformation("[AGGREGATION] Worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var keys = await db.SetMembersAsync("chat_keys");

                    _logger.LogInformation($"[REDIS] Scan {keys.Length} keys");

                    foreach (var keyValue in keys)
                    {
                        if (!keyValue.HasValue)
                        {
                            _logger.LogWarning("[REDIS] Empty keyValue → skip");
                            continue;
                        }

                        RedisKey key = keyValue.ToString();
                        var lastKey = $"last:{key}";

                        _logger.LogInformation($"[REDIS] Checking key={key}");

                        
                        var lastValue = await db.StringGetAsync(lastKey);

                        if (!lastValue.HasValue)
                        {
                            _logger.LogWarning($"[REDIS] Missing lastKey={lastKey} → skip");
                            continue;
                        }

                        var lastTime = new DateTime((long)lastValue);
                        var diff = DateTime.UtcNow - lastTime;

                        _logger.LogInformation($"[DEBOUNCE] key={key} idle={diff.TotalSeconds}s");

                        // debounce 60s
                        if (diff < TimeSpan.FromSeconds(60))
                        {
                            _logger.LogInformation($"[DEBOUNCE] Still active → skip key={key}");
                            continue;
                        }

                        var lockKey = $"lock:{key}";

                        var isLocked = await db.StringSetAsync(
                            lockKey,
                            "1",
                            TimeSpan.FromSeconds(10),
                            When.NotExists
                        );

                        if (!isLocked)
                        {
                            _logger.LogInformation($"[LOCK] Already locked → skip key={key}");
                            continue;
                        }

                        _logger.LogInformation($"[LOCK] Acquired {lockKey}");

                        try
                        {
                            var messages = await db.ListRangeAsync(key);

                            _logger.LogInformation($"[REDIS] Read {messages.Length} messages from key={key}");

                            if (messages.Length == 0)
                            {
                                _logger.LogWarning($"[REDIS] Empty messages → cleanup key={key}");

                                await db.KeyDeleteAsync(key);
                                await db.KeyDeleteAsync(lastKey);
                                await db.SetRemoveAsync("chat_keys", key.ToString());
                                continue;
                            }

                            var keyStr = key.ToString();
                            var parts = keyStr.Split(':');

                            if (parts.Length != 3)
                            {
                                _logger.LogWarning($"[REDIS] Invalid key format → delete key={key}");

                                await db.KeyDeleteAsync(key);
                                await db.KeyDeleteAsync(lastKey);
                                await db.SetRemoveAsync("chat_keys", key.ToString());
                                continue;
                            }

                            if (!Guid.TryParse(parts[1], out var providerId) ||
                                !Guid.TryParse(parts[2], out var customerId))
                            {
                                _logger.LogWarning($"[REDIS] Invalid GUID → delete key={key}");

                                await db.KeyDeleteAsync(key);
                                await db.KeyDeleteAsync(lastKey);
                                await db.SetRemoveAsync("chat_keys", key.ToString());
                                continue;
                            }

                            using var scope = _scopeFactory.CreateScope();

                            var conversationService = scope.ServiceProvider
                                .GetRequiredService<ISupportConversationService>();

                            var taskService = scope.ServiceProvider
                                .GetRequiredService<ITaskAssignmentService>();

                            var conversation = await conversationService
                                .GetSupportConversationHavePendingByCustomerIdAsync(customerId, providerId);

                            if (conversation == null)
                            {
                                _logger.LogWarning($"[BUSINESS] Conversation null → cleanup key={key}");

                                await db.KeyDeleteAsync(key);
                                await db.KeyDeleteAsync(lastKey);
                                await db.SetRemoveAsync("chat_keys", key.ToString());
                                continue;
                            }

                            if (conversation.IsDistributed)
                            {
                                _logger.LogInformation($"[BUSINESS] Already distributed → cleanup key={key}");

                                await db.KeyDeleteAsync(key);
                                await db.KeyDeleteAsync(lastKey);
                                await db.SetRemoveAsync("chat_keys", key.ToString());
                                continue;
                            }

                            var text = string.Join(" ", messages.Select(x => x.ToString()));

                            _logger.LogInformation($"[AGGREGATION] Processing key={key} | length={text.Length}");

                            _logger.LogInformation($"[AGGREGATION] TEXT = {text}");


                            var predictReqet = new PredictRequest
                            {
                                Message = text,
                            };

                            await taskService.ProcessTask(predictReqet, conversation.Id);

                            _logger.LogInformation($"[AGGREGATION] Done key={key}");

                            // cleanup
                            await db.KeyDeleteAsync(key);
                            await db.KeyDeleteAsync(lastKey);
                            await db.SetRemoveAsync("chat_keys", key.ToString());

                            _logger.LogInformation($"[REDIS] Deleted key + lastKey={key}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"[AGGREGATION] Error processing key={key}");
                        }
                        finally
                        {
                            await db.KeyDeleteAsync(lockKey);
                            _logger.LogInformation($"[LOCK] Released {lockKey}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AGGREGATION] Loop error");
                }

                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("[AGGREGATION] Worker stopped");
        }
    }
}
