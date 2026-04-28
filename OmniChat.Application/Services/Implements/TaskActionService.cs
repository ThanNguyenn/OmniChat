using Amazon.Runtime;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.TaskAction;
using OmniChat.Infrastructure.Dtos.Responses.TaskAction;
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
    public class TaskActionService : BaseService<TaskActionService>, ITaskActionService
    {
        public TaskActionService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<TaskActionService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<PagingResponse<TaskActionResponse>> GetAllTaskActionAsync(int pageIndex = 1, int pageSize = 10)
        {
            var repo = _unitOfWork.GetRepository<TaskAction>();

            var query = repo.GetQueryable()
                .OrderByDescending(t => t.CreateDate);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<TaskActionResponse>>(items);

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PagingResponse<TaskActionResponse>
            {
                Items = mapped,
                Meta = new PaginationMeta
                {
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }


        public async Task<TaskActionResponse> GetTaskActionByIdAsync(Guid id)
        {
            var taskActionRepo = _unitOfWork.GetRepository<TaskAction>();
            var taskAction = await taskActionRepo.SingleOrDefaultAsync(predicate: x => x.Id == id);
            if (taskAction == null)
            {
                throw new NotFoundException("Không tìm thấy hành động công việc yêu cầu.");
            }
            return _mapper.Map<TaskActionResponse>(taskAction);
        }

        public async Task<bool> CreateTaskActionAsync(TaskActionRequest actionRequest)
        {
            var taskActionRepo = _unitOfWork.GetRepository<TaskAction>();

            var taskAction = _mapper.Map<TaskAction>(actionRequest);

            await taskActionRepo.InsertAsync(taskAction);

            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}
