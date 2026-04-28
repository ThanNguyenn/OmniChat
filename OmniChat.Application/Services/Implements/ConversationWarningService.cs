using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.WarningConversation;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
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
    public class ConversationWarningService : BaseService<ConversationWarningService>, IConversationWarningService
    {


        public ConversationWarningService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<ConversationWarningService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<PagingResponse<WarningDetailRepsone>> GetAllWarningsAsync(
         int pageNumber = 1,
         int pageSize = 10,
         bool? isReviewed = null)
        {
            var warningRepo = _unitOfWork.GetRepository<ConversationWarning>();

           
            var pagingResult = await warningRepo.GetPagingListAsync(
                selector: w => new WarningDetailRepsone
                {
                    Id = w.Id,
                    CustomerName = w.Conversation.CustomerProfile.CustomerName,
                    StaffName = w.Staff.Name,
                    CreateAt = w.CreatedAt,
                    WarningType = w.WarningType,
                    Reason = w.Reason,
                    IsReviewed = w.IsReviewed
                },
                predicate: w => isReviewed == null || w.IsReviewed == isReviewed,
                include: q => q
                    .Include(w => w.Staff)
                    .Include(w => w.Conversation).ThenInclude(c => c.CustomerProfile),
                orderBy: q => q.OrderByDescending(w => w.CreatedAt),
                page: pageNumber,
                size: pageSize
            );

          
            return pagingResult;
        }

        public async Task<WarningDetailRepsone> GetWarningByIdAsync(Guid id)
        {
            var warningRepo = _unitOfWork.GetRepository<ConversationWarning>();

            var warning = await warningRepo.SingleOrDefaultAsync(
                predicate: w => w.Id == id,
                include: q => q.Include(w => w.Staff)
                               .Include(w => w.Conversation).ThenInclude(c => c.CustomerProfile)
            );

            if (warning == null)
                throw new NotFoundException($"Không tìm thấy cảnh báo với mã định danh: {id}");

            var response = new WarningDetailRepsone
            {
                Id = warning.Id,
                CustomerName = warning.Conversation?.CustomerProfile?.CustomerName,
                StaffName = warning.Staff?.Name,
                CreateAt = warning.CreatedAt,
                WarningType = warning.WarningType,
                Reason = warning.Reason,
                IsReviewed = warning.IsReviewed
            };

            
            if (!warning.IsReviewed)
            {
                warning.IsReviewed = true;
                warningRepo.Update(warning);
                await _unitOfWork.CommitAsync();
            }

            return response;
        }

        public async Task DeleteWarningAsync()
        {
            var warningRepo = _unitOfWork.GetRepository<ConversationWarning>();
            var limitDate = DateTime.UtcNow.AddDays(-30);

            var warnings = await warningRepo.GetListAsync(
                predicate: x => x.IsReviewed == true && x.CreatedAt <= limitDate
            );

            if (warnings != null && warnings.Any())
            {
                warningRepo.DeleteRange(warnings);
                await _unitOfWork.CommitAsync();
            }
        }
    }
}
