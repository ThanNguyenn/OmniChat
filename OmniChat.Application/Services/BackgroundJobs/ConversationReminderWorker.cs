using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
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

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task ProcessReminder()
        {
            using var scope = _serviceProvider.CreateScope();

            var conversationService = scope.ServiceProvider
                .GetRequiredService<ISupportConversationService>();

            var messageService = scope.ServiceProvider
                .GetRequiredService<ISupportStaffMessageService>();

            var now = DateTime.UtcNow;

            var conversations = await conversationService.GetConversationsForReminderAsync();

            foreach (var convo in conversations)
            {
                if (convo.LastStaffMessageAt == null)
                    continue;

                if (!CustomerNotReplied(convo)) continue;

                var diff = now - convo.LastStaffMessageAt.Value;

                bool needUpdate = false;

                if (convo.Status == ConversationStatus.Complete)
                    continue;

                // For testing, set to 5 minutes
                //if (diff.TotalHours >= 23 && !convo.ReminderSent)
                if (diff.TotalMinutes >= 5 && !convo.ReminderSent)
                {
                    await SendReminder(convo, messageService);
                    convo.ReminderSent = true;
                    needUpdate = true;
                }

                // close conversation if customer does not reply after 10 minutes of staff message
                //   if (diff.TotalHours >= 24 && convo.ReminderSent && CustomerNotReplied(convo))
                if (diff.TotalMinutes >= 10 && convo.ReminderSent && CustomerNotReplied(convo))
                {
                    convo.Status = ConversationStatus.Complete;
                    convo.CloseAt = now;
                    needUpdate = true;
                }

                if (needUpdate)
                    await conversationService.UpdateConversationAsync(convo);
            }
        }

        private bool CustomerNotReplied(SupportConversation convo)
        {
            if (convo.LastStaffMessageAt == null) return false;

            if (convo.LastCustomerMessageAt == null)
                return true;

            return convo.LastCustomerMessageAt <= convo.LastStaffMessageAt;
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
                _logger.LogInformation("Sending Facebook reminder");
                await messageService.SendFacebookMesageAsync(new CreateSupportStaffMessageRequest
                {
                    SupportConversationId = convo.Id,
                    Content = content,
                    StaffId = convo.ActiveStaffId.Value
                });
            }
        }
    }
}
