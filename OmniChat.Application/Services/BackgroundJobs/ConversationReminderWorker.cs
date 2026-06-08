using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Mail;
using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.BackgroundJobs
{
    public class ConversationReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConversationReminderWorker> _logger;

        public ConversationReminderWorker(IServiceProvider serviceProvider, ILogger<ConversationReminderWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Conversation Reminder Job started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessReminder();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Reminder Job");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

    
        private async Task ProcessReminder()
        {
            List<SupportConversation> conversations;
            using (var scope = _serviceProvider.CreateScope())
            {
                var conversationService = scope.ServiceProvider.GetRequiredService<ISupportConversationService>();
                conversations = await conversationService.GetConversationsForReminderAsync();
            }

            var now = DateTime.UtcNow;

            foreach (var convo in conversations)
            {
                try
                {
                    bool needUpdate = false;

                    if (StaffNotReplied(convo))
                    {
                        if (convo.LastCustomerMessageAt == null) continue;
                        var staffDelay = now - convo.LastCustomerMessageAt.Value;

                        if (staffDelay.TotalHours >= 22 && convo.IsWarningSent != true)
                        {
                            _logger.LogWarning("[WARNING] Staff {StaffId} delayed > 22h for Convo {Id}", convo.ActiveStaffId, convo.Id);

                            using var warningScope = _serviceProvider.CreateScope();

                            var mailService = warningScope.ServiceProvider.GetRequiredService<IMailService>();
                            if (convo.Staff != null && !string.IsNullOrEmpty(convo.Staff.Email))
                            {
                                await mailService.SendEmailAsync(new MailContent
                                {
                                    To = convo.Staff.Email,
                                    Subject = $"[CẢNH BÁO] Phản hồi khách hàng chậm trễ - Hội thoại #{convo.CustomerName}",
                                    Body = $@"
                                <h3>Thông báo nhắc nhở công việc</h3>
                                <p>Chào <b>{convo.Staff.Name ?? "Bạn"}</b>,</p>
                                <p>Hội thoại với khách hàng <b>{convo.CustomerName}</b> đang bị trì hoãn quá 24 giờ chưa có phản hồi từ bạn.</p>
                                <p>Vui lòng kiểm tra ngay.</p>"
                                });
                                _logger.LogInformation("Sent warning email to staff: {Email}", convo.Staff.Email);
                            }

                            var messageService = warningScope.ServiceProvider.GetRequiredService<ISupportStaffMessageService>();
                            await SendApologyMessage(convo, messageService);

                            convo.IsWarningSent = true;
                            needUpdate = true;
                        }
                    }


                    else if (CustomerNotReplied(convo))
                    {
                       if (convo.LastStaffMessageAt == null) continue;

                        var hoursSinceStaffMessage = (now - convo.LastStaffMessageAt.Value).TotalHours;

                        if (convo.LastCustomerMessageAt == null) continue; 
                        var hoursSinceCustomer = (now - convo.LastCustomerMessageAt.Value).TotalHours;

                        if (hoursSinceCustomer >= 24 && convo.ReminderSent == true)
                        {
                            _logger.LogInformation("Closing conversation ID: {Id} due to customer inactivity", convo.Id);
                            convo.Status = ConversationStatus.Complete;
                            convo.CloseAt = now;
                            needUpdate = true;

                            using var taskScope = _serviceProvider.CreateScope();
                            var performanceService = taskScope.ServiceProvider.GetRequiredService<IStaffPerformanceService>();
                            await performanceService.CompleteConversationAndTasksAsync(convo);
                        }
 
                        else if (hoursSinceStaffMessage >= 22 && convo.ReminderSent != true)
                        {
                            _logger.LogInformation("Sending reminder to customer for ID: {Id}", convo.Id);
                            using var sendScope = _serviceProvider.CreateScope();
                            var messageService = sendScope.ServiceProvider.GetRequiredService<ISupportStaffMessageService>();

                            await SendReminder(convo, messageService);
                            convo.ReminderSent = true;
                            needUpdate = true;
                        }
                    }

                    if (needUpdate)
                    {
                        using var updateScope = _serviceProvider.CreateScope();
                        var conversationService = updateScope.ServiceProvider.GetRequiredService<ISupportConversationService>();

                        var freshConvo = await conversationService.GetSupportConversationByIdAsync(convo.Id);
                        if (freshConvo != null)
                        {
                            freshConvo.IsWarningSent = convo.IsWarningSent;
                            freshConvo.ReminderSent = convo.ReminderSent;
                            freshConvo.Status = convo.Status;
                            freshConvo.CloseAt = convo.CloseAt;

                            await conversationService.UpdateConversationAsync(freshConvo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing ConvoId={Id}", convo.Id);
                }
            }
        }

        private bool CustomerNotReplied(SupportConversation convo)
        { 
            // staff rep 
            if (convo.LastStaffMessageAt == null) return false;

            // customer chưa rep 
            if (convo.LastCustomerMessageAt == null) return true;

            // tin cuoi la cua staff
            return convo.LastCustomerMessageAt < convo.LastStaffMessageAt;
        }

        private bool StaffNotReplied(SupportConversation convo)
        {

            //  customer rep
            if (convo.LastCustomerMessageAt == null) return false;

            // Staff chưa rep lan nao
            if (convo.LastStaffMessageAt == null) return true;
            // tin cuoi la cua customer
            return convo.LastStaffMessageAt < convo.LastCustomerMessageAt;
        }

        private async Task SendReminder(SupportConversation convo, ISupportStaffMessageService messageService)
        {
            _logger.LogInformation("SendReminder triggered | ConversationId: {Id}", convo.Id);

            if (convo.Providers == null || convo.ActiveStaffId == null)
            {
                _logger.LogWarning("Cannot send reminder due to missing data");
                return;
            }

            var content = "Hi bạn, bên mình vẫn đang hỗ trợ. Bạn cần thêm gì không?";

            if (convo.Providers.ProviderName == "Zalo")
            {
                _logger.LogInformation("Sending Zalo reminder");
                await messageService.SendZaloMessageAsync(new CreateSupportStaffMessageRequest
                {
                    SupportConversationId = convo.Id,
                    Content = content,
                    StaffId = convo.ActiveStaffId.Value
                });
            }
            else if (convo.Providers.ProviderName == "Facebook")
            {
                
                if (convo.LastCustomerMessageAt == null)
                {
                    _logger.LogWarning(
                        "[REMINDER] Skip Facebook ConvoId={Id} | Reason: No customer message found",
                        convo.Id
                    );
                    return;
                }

                var hoursSinceCustomerMessage = (DateTime.UtcNow - convo.LastCustomerMessageAt.Value).TotalHours;

                if (hoursSinceCustomerMessage >= 24)
                {
                    _logger.LogWarning(
                        "[REMINDER] Skip Facebook ConvoId={Id} | Reason: Outside 24h window | LastCustomerMessage={LastMsg} ({Hours:F1}h ago)",
                        convo.Id, convo.LastCustomerMessageAt, hoursSinceCustomerMessage
                    );
                    return; // Không gửi, không throw exception
                }

                _logger.LogInformation("Sending Facebook reminder");
                await messageService.SendFacebookMesageAsync(new CreateSupportStaffMessageRequest
                {
                    SupportConversationId = convo.Id,
                    Content = content,
                    StaffId = convo.ActiveStaffId.Value
                });
            }
        }

        private async Task SendApologyMessage(SupportConversation convo, ISupportStaffMessageService messageService)
        {
            var content = "Xin lỗi bạn, hiện tại tư vấn viên đang bận nên phản hồi hơi chậm. Chúng mình sẽ quay lại hỗ trợ bạn ngay!";

            var request = new CreateSupportStaffMessageRequest
            {
                SupportConversationId = convo.Id,
                Content = content,
                StaffId = convo.ActiveStaffId ?? Guid.Empty
            };

            if (convo.Providers?.ProviderName == "Zalo")
            {
                await messageService.SendZaloMessageAsync(request);
            }
            else if (convo.Providers?.ProviderName == "Facebook")
            {
                if (convo.LastCustomerMessageAt == null) return;
                //var hoursSinceLastCustomerMsg = (DateTime.UtcNow - convo.LastCustomerMessageAt.Value).TotalHours;
                //if (hoursSinceLastCustomerMsg < 24)
                //{
                    await messageService.SendFacebookMesageAsync(request);
                //}
            }
        }
    }
}
