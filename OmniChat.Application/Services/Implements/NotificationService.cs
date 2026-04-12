using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Notification;
using OmniChat.Infrastructure.Dtos.Responses.Notification;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class NotificationService : BaseService<NotificationService>, INotificationService
    {
        public NotificationService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<NotificationService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<bool> CreateNotificationAsync(NotificationRequest request)
        {
            var repo = _unitOfWork.GetRepository<Notification>();

            var entity = _mapper.Map<Notification>(request);

            await repo.InsertAsync(entity);
            await _unitOfWork.CommitAsync();
            return true;          
        }

        public async Task<IEnumerable<NotificationResponse>> GetNotificationsByStaffIdAsync(Guid staffId)
        {
            var converRepo = _unitOfWork.GetRepository<SupportConversation>();

            var staffConversations = await converRepo.GetListAsync(
           predicate: x => x.ActiveStaffId == staffId,
            include: source => source
            .Include(c => c.Providers)
            .Include(c => c.CustomerMessages.Where(m => m.IsRead == false))
            );


            var unreadNotifications = staffConversations
                    .SelectMany(c => c.CustomerMessages.Select(m => new NotificationResponse
                    {
                        Message = m.Content,
                        CustomerName = c.CustomerName,
                        ImageUrl = c.AvatarUrl,
                        ProviderName = c.Providers?.ProviderName ?? "Unknown",
                        TimeStamp = m.Timestamp
                    }))
                    .OrderByDescending(n => n.TimeStamp) // Sắp xếp tin nhắn mới nhất lên đầu
                    .ToList();

            return unreadNotifications;
        }

        // call when call getConversationDetail Api
        public async Task UpdateNotificationIsReadAsync(Guid conversationId) 
        {
            var notificationRepo = _unitOfWork.GetRepository<Notification>();

            var notifications = await notificationRepo.GetListAsync(predicate: x => x.ConversationId == conversationId && x.IsRead == false);

            foreach (var item in notifications)
            {
              item.IsRead = true;
                notificationRepo.Update(item);
            }
            await _unitOfWork.CommitAsync();
        }
    }
}
