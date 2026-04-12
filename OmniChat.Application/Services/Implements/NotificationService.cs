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
           
            var notificationRepo = _unitOfWork.GetRepository<Notification>();

            
            var notifications = await notificationRepo.GetListAsync(
                predicate: n => n.StaffId == staffId && n.IsRead == false,
                include: source => source
                    .Include(n => n.SupportConversation)
                        .ThenInclude(sc => sc.Providers),
                orderBy: q => q.OrderByDescending(n => n.CreatedDate) 
            );

         
            var response = notifications.Select(n => new NotificationResponse
            {
                Message = n.MessageText,
                CustomerName = n.SupportConversation?.CustomerName ?? "Unknown",
                ImageUrl = n.SupportConversation?.AvatarUrl,
                ProviderName = n.SupportConversation?.Providers?.ProviderName ?? "Unknown",
                CreatedDate = n.CreatedDate ?? DateTime.UtcNow,
                TimeStamp = n.CreatedDate.HasValue
                            ? new DateTimeOffset(n.CreatedDate.Value).ToUnixTimeMilliseconds()
                            : 0
            });

            return response;
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
