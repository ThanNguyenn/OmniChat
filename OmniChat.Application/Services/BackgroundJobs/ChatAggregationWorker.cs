using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Requests.Intent;
using OmniChat.Infrastructure.Dtos.Requests.Notification;
using OmniChat.Infrastructure.Dtos.Responses.Notification;
using OmniChat.Infrastructure.Models;
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
                        if (DateTime.UtcNow - lastTime < TimeSpan.FromSeconds(5)) continue;


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


                            var conversation = await conversationService.GetSupportConversationHavePendingByCustomerIdAsync(customerId, providerId);
                            if (conversation == null || conversation.IsDistributed) { await CleanupRedis(db, keyStr, lastKey); continue; }


                            var text = string.Join(" ", messages.Select(x => x.ToString()));
                            _logger.LogInformation("[AGGREGATION] Calling AI for Customer: {Id}", customerId);

                            var haveActiveStaff = await taskService.ProcessTask(new PredictRequest { Message = text }, conversation.Id);

                            var updatedConv = await conversationService.GetSupportConversationByIdAsync(conversation.Id);


                            //  has free staff and has task
                            if (updatedConv?.ActiveStaffId != null && updatedConv.SupportTasks.Any())
                            {
                                _logger.LogInformation("[AGGREGATION] Staff assigned: {Staff}", updatedConv.ActiveStaffId);

                                // send last message notification to staff
                                string lastMessageContent = "Bạn có cuộc hội thoại mới đang chờ!";
                                long finalTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                                using (var internalScope = _scopeFactory.CreateScope())
                                {
                                    var messageService = internalScope.ServiceProvider.GetRequiredService<ICustomerMessageService>();
                                    var notificationService = internalScope.ServiceProvider.GetRequiredService<INotificationService>();
                                   

                                    var lastDbMessage = await messageService.GetLastMessageByConversationIdAsync(updatedConv.Id);
                                    if (lastDbMessage != null)
                                    {
                                        lastMessageContent = lastDbMessage.Content;
                                        finalTimestamp = lastDbMessage.Timestamp;
                                    }

                                    var notificationReq = new NotificationRequest
                                    {
                                        ConversationId = updatedConv.Id,
                                        StaffId = updatedConv.ActiveStaffId.Value,
                                        MessageText = lastMessageContent,
                                        IsRead = false
                                    };

                                    await notificationService.CreateNotificationAsync(notificationReq);
                                }

                                var notificationResponse = new NotificationResponse
                                {
                                    CustomerName = updatedConv.CustomerName,
                                    ImageUrl = updatedConv.AvatarUrl,
                                    Message = lastMessageContent,
                                    ProviderName = updatedConv.Providers.ProviderName,
                                    TimeStamp = finalTimestamp,
                                };

                                await _hubContext.Clients
                                .User(conversation.ActiveStaffId.ToString())
                                .SendAsync("ReceiveNotification", notificationResponse);


                                _logger.LogError("[AGGREGATION] Before singnalR ");
                                await conversationService.PushSidebarToStaffAsync(updatedConv.ActiveStaffId.Value, updatedConv.Providers.ProviderName);
                            }
                            // no task 
                            else if (!updatedConv.SupportTasks.Any())
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
