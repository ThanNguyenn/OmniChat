using Amazon.Runtime;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.TaskAction;
using OmniChat.Infrastructure.Dtos.Responses.TaskAction;
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
    public class TaskActionService : BaseService<TaskActionService>, ITaskActionService
    {
        public TaskActionService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<TaskActionService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<PaginatedResponse<TaskActionResponse>> GetAllTaskActionAsync()
        {
            var taskActionRepo = _unitOfWork.GetRepository<TaskAction>();

            var taskActions = await taskActionRepo.GetListAsync();

            var response = taskActions.Select(taskAction => _mapper.Map<TaskActionResponse>(taskAction)).ToList();

            return new PaginatedResponse<TaskActionResponse>
            {
                Items = response,
                TotalCount = response.Count
            };
        }


        public async Task<TaskActionResponse> GetTaskActionByIdAsync(Guid id)
        {
            var taskActionRepo = _unitOfWork.GetRepository<TaskAction>();
            var taskAction = await taskActionRepo.SingleOrDefaultAsync(predicate: x => x.Id == id);
            if (taskAction == null)
            {
                throw new NotFoundException("Task Action not found");
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
