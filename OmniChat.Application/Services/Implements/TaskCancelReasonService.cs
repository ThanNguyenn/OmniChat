using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.TaskCancelReason;
using OmniChat.Infrastructure.Dtos.Responses.TaskCancelReason;
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
    public class TaskCancelReasonService : BaseService<TaskCancelReasonService>, ITaskCancelReasonService
    {
        public TaskCancelReasonService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<TaskCancelReasonService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<PagingResponse<TaskCancelReasonResponse>> GetAllTaskCancelReasonAsync(int page = 1, int pageSize = 10)
        {
            var cancelReasonRepo = _unitOfWork.GetRepository<TaskCancelReason>();

            var cancelReasons = await cancelReasonRepo.GetListAsync(
                orderBy: x => x.OrderByDescending(c => c.CreateDate)
            );

            var totalItems = cancelReasons.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedItems = cancelReasons
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var mapped = _mapper.Map<IEnumerable<TaskCancelReasonResponse>>(pagedItems);

            return new PagingResponse<TaskCancelReasonResponse>
            {
                Items = mapped,
                Meta = new PaginationMeta
                {
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            };
        }


        public async Task<TaskCancelReasonResponse> GetTaskCancelReasonBySupportTaskIdAsync(Guid supportTaskId)
        {
            var cancelReasonRepo = _unitOfWork.GetRepository<TaskCancelReason>();
            var cancelReason = await cancelReasonRepo.SingleOrDefaultAsync(predicate: x => x.SupportTaskId == supportTaskId);
            if (cancelReason == null)
            {
                throw new NotFoundException("Không tìm thấy lý do hủy cho công việc này.");
            }
            return _mapper.Map<TaskCancelReasonResponse>(cancelReason);
        }

    }
}
