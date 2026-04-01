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
                if (!CustomerNotReplied(convo)) continue;

                var diff = now - convo.LastStaffMessageAt.Value;

                bool needUpdate = false;

                if (diff.TotalHours >= 22 && diff.TotalHours < 24)
                {
                    if (!convo.ReminderSent)
                    {
                        convo.ReminderSent = true;
                        needUpdate = true;

                        await conversationService.UpdateConversationAsync(convo);

                        await SendReminder(convo, messageService);
                    }
                }

                if (diff.TotalHours >= 24)
                {
                    convo.Status = ConversationStatus.Complete;
                    convo.CloseAt = now;
                    needUpdate = true;

                    // not performace yet
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

            return convo.LastCustomerMessageAt < convo.LastStaffMessageAt;
        }

        private async Task SendReminder(SupportConversation convo, ISupportStaffMessageService messageService)
        {
            if (convo.Providers == null || convo.ActiveStaffId == null)
                return;

            var content = "Hi bạn, bên mình vẫn đang hỗ trợ. Bạn cần thêm gì không?";

            if (convo.Providers.ProviderName == "Zalo")
            {
                await messageService.SendZaloMessageAsync(new CreateSupportStaffMessageRequest
                {
                    SupportConversationId = convo.Id,
                    Content = content,
                    StaffId = convo.ActiveStaffId.Value
                });
            }
            else if (convo.Providers.ProviderName == "Facebook")
            {
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
