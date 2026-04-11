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

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var keys = await db.SetMembersAsync("chat_keys");

                    foreach (var keyValue in keys)
                    {
                        if (!keyValue.HasValue) continue;

                        string keyStr = keyValue.ToString();
                        var lastKey = $"last:{keyStr}";
                        var lockKey = $"lock:{keyStr}";

                  
                        var lastValue = await db.StringGetAsync(lastKey);
                        if (!lastValue.HasValue) continue;

                        var lastTime = new DateTime((long)lastValue);
                        if (DateTime.UtcNow - lastTime < TimeSpan.FromSeconds(60)) continue;

                      
                        var isLocked = await db.StringSetAsync(lockKey, "1", TimeSpan.FromSeconds(30), When.NotExists);
                        if (!isLocked) continue;

                        try
                        {
                            var messages = await db.ListRangeAsync(keyStr);
                            if (messages.Length == 0) { await CleanupRedis(db, keyStr, lastKey); continue; }

                            var parts = keyStr.Split(':');
                            if (parts.Length != 3 || !Guid.TryParse(parts[1], out var providerId) || !Guid.TryParse(parts[2], out var customerId))
                            {
                                await CleanupRedis(db, keyStr, lastKey); continue;
                            }

                            using var scope = _scopeFactory.CreateScope();
                            var conversationService = scope.ServiceProvider.GetRequiredService<ISupportConversationService>();
                            var taskService = scope.ServiceProvider.GetRequiredService<ITaskAssignmentService>();
                            var mergeService = scope.ServiceProvider.GetRequiredService<ICustomerMergeService>();

                          
                            var conversation = await conversationService.GetSupportConversationHavePendingByCustomerIdAsync(customerId, providerId);
                            if (conversation == null || conversation.IsDistributed) { await CleanupRedis(db, keyStr, lastKey); continue; }

                       
                            var text = string.Join(" ", messages.Select(x => x.ToString()));
                            _logger.LogInformation("[AGGREGATION] Calling AI for Customer: {Id}", customerId);

                           var haveActiveStaff = await taskService.ProcessTask(new PredictRequest { Message = text }, conversation.Id);

                            var updatedConv = await conversationService.GetSupportConversationByIdAsync(conversation.Id);



                            if (updatedConv?.ActiveStaffId != null)
                            {
                                _logger.LogInformation("[AGGREGATION] Staff assigned: {Staff}. Sending Link...", updatedConv.ActiveStaffId);
                              
                                await mergeService.SendFormLinkIfNeededAsync(updatedConv);
                            }
                            else
                            {
                                _logger.LogInformation("[AGGREGATION] No staff found for conversation: {Id}. Sending system guide.", conversation.Id);
                                string guideMessage = "Vui lòng nhập các tin nhắn liên quan như : Đặt Hàng , tư vấn sản phẩm, kiểm tra trạng thái đơn hàng, kiểm tra công nợ !";
                                var messageService = scope.ServiceProvider.GetRequiredService<ISupportStaffMessageService>();
                                await messageService.SendSystemMessageToExternalAsync(updatedConv.Id, guideMessage);
                            }

                           
                            await CleanupRedis(db, keyStr, lastKey);
                            _logger.LogInformation("[AGGREGATION] Done & Cleaned: {Key}", keyStr);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[AGGREGATION] Error processing key={Key}", keyStr);
                           
                            await CleanupRedis(db, keyStr, lastKey);
                        }
                        finally
                        {
                            await db.KeyDeleteAsync(lockKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AGGREGATION] Global loop error");
                }
                await Task.Delay(2000, stoppingToken);
            }
        }

        private async Task CleanupRedis(IDatabase db, string key, string lastKey)
        {
            await db.KeyDeleteAsync(key);
            await db.KeyDeleteAsync(lastKey);
            await db.SetRemoveAsync("chat_keys", key);
        }
    }
}
