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

            _logger.LogInformation("[NOTIFICATION] Mapping NotificationRequest for ConversationId: {ConvId}", request.ConversationId);

            var entity = _mapper.Map<Notification>(request);

            _logger.LogInformation("[NOTIFICATION] Entity Mapped: StaffId={StaffId}, ConvId={ConvId}, CreatedDate={Date}",
            entity.StaffId, entity.ConversationId, entity.CreatedDate);

            await repo.InsertAsync(entity);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("[NOTIFICATION] Inserted successfully to DB. ID: {Id}", entity.Id);
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
                CustomerName = n.SupportConversation?.CustomerName ?? "Khách ẩn danh",
                ImageUrl = n.SupportConversation?.AvatarUrl,
                ProviderName = n.SupportConversation?.Providers?.ProviderName ?? "Hệ thống",
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
            if (conversationId == Guid.Empty) 
                throw new BadRequestException("Mã cuộc hội thoại không hợp lệ.");

            var notificationRepo = _unitOfWork.GetRepository<Notification>();           
            var notifications = await notificationRepo.GetListAsync(
                predicate: x => x.ConversationId == conversationId && x.IsRead == false
            );
            if (notifications.Any())
            {            
                foreach (var item in notifications)
                {
                    item.IsRead = true;
                }
              
                notificationRepo.UpdateRange(notifications);             
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task DeleteNofiticationIsReadAsync()
        {
            var notificationRepo = _unitOfWork.GetRepository<Notification>();
            var notifications = await notificationRepo.GetListAsync(
                predicate: x =>  x.IsRead == true
            );

            notificationRepo.DeleteRange(notifications);
            await _unitOfWork.CommitAsync();
        }
    }
}
