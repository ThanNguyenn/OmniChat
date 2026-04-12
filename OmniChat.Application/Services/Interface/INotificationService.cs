using OmniChat.Infrastructure.Dtos.Requests.Notification;
using OmniChat.Infrastructure.Dtos.Responses.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface INotificationService
    {
        public  Task<bool> CreateNotificationAsync(NotificationRequest request);

        public  Task<IEnumerable<NotificationResponse>> GetNotificationsByStaffIdAsync(Guid staffId);

        public  Task UpdateNotificationIsReadAsync(Guid conversationId);

        public  Task DeleteNofiticationIsReadAsync();

    }
}
