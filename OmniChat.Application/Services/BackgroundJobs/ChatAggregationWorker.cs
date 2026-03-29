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
        private readonly ISupportConversationService _supportConversationService;
        private readonly ITaskAssignmentService _taskAssignmentService;
        public ChatAggregationWorker(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory,
        ILogger<ChatAggregationWorker> logger,ISupportConversationService supportConversationService,
        ITaskAssignmentService taskAssignmentService,
         IHubContext<SupportConversationHub> hubContext
        )
        {
            _redis = redis;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _supportConversationService = supportConversationService;
            _taskAssignmentService = taskAssignmentService;
            _hubContext = hubContext;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = _redis.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var server = _redis.GetServer(_redis.GetEndPoints().First());

                    foreach (var key in server.Keys(pattern: "chat:*"))
                    {
                        var ttl = await db.KeyTimeToLiveAsync(key);

                        if (ttl.HasValue && ttl.Value <= TimeSpan.Zero)
                        {
                            var lockKey = $"lock:{key}";
                            var isLocked = await db.StringSetAsync(lockKey, "1", TimeSpan.FromSeconds(5), When.NotExists);

                            if (!isLocked)
                                continue;

                            try
                            {
                                var messages = await db.ListRangeAsync(key);

                                if (messages.Length == 0)
                                    continue;

                                var keyStr = key.ToString();
                                var parts = keyStr.Split(':');

                                if (parts.Length != 3)
                                {
                                    await db.KeyDeleteAsync(key);
                                    continue;
                                }

                                if (!Guid.TryParse(parts[1], out var providerId) ||
                                    !Guid.TryParse(parts[2], out var customerId))
                                {
                                    await db.KeyDeleteAsync(key);
                                    continue;
                                }

                                using var scope = _scopeFactory.CreateScope();

                                var conversationService = scope.ServiceProvider
                                    .GetRequiredService<ISupportConversationService>();

                                var conversation = await conversationService
                                    .GetSupportConversationHavePendingByCustomerIdAsync(customerId, providerId);

                                if (conversation == null || conversation.IsDistributed)
                                {
                                    await db.KeyDeleteAsync(key);
                                    continue;
                                }

                                var text = string.Join(" ", messages.Select(x => x.ToString()));

                                _logger.LogInformation($"[AGGREGATION] Processing {key}");

                                //  call AI
                                var predictReqet = new PredictRequest
                                {
                                    Message = text,
                                };

                                await _taskAssignmentService.ProcessTask(predictReqet, conversation.Id);


                                await db.KeyDeleteAsync(key);
                            }
                            finally
                            {
                                await db.KeyDeleteAsync(lockKey);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AGGREGATION] Error");
                }

                await Task.Delay(500, stoppingToken);
            }
        }
    }
}
