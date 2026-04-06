using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.SupportTask;
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
    public class SupportTaskService : BaseService<SupportTaskService>, ISupportTaskService
    {
        private readonly IStaffPerformanceService _staffPerformanceService;

        public SupportTaskService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<SupportTaskService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IStaffPerformanceService staffPerformanceService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _staffPerformanceService = staffPerformanceService;
        }

        public async Task<IEnumerable<SupportTask>> GetDoneSupportTaskByConversationIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportTask>();
            var supportTasks = await repo.GetListAsync(
                predicate: x => x.SupportConversationId == conversationId
                && x.Status == SupportTaskStatus.Done);

            if (!supportTasks.Any())
            {
                throw new NotFoundException("No SupportTask Found");
            }

            return supportTasks;
        }

        public async Task<IEnumerable<SupportTasksResponse>> GetSupportTaskOnConversationIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportTask>();

            var activeStatuses = new[]
            {
        SupportTaskStatus.Done,
        SupportTaskStatus.InProgress,
        SupportTaskStatus.PendingReassign
              };

            var supportTasks = await repo.GetListAsync(
                predicate: x => x.SupportConversationId == conversationId
                             && activeStatuses.Contains(x.Status),
                include: x => x.Include(x => x.IntentType)
            );


            if (!supportTasks.Any())
                return Enumerable.Empty<SupportTasksResponse>();

            return _mapper.Map<IEnumerable<SupportTasksResponse>>(supportTasks);
        }


        public async Task<IEnumerable<SupportTask>> GetSupportTaskByConversationIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportTask>();

            var supportTasks = await repo.GetListAsync(
                predicate: x => x.SupportConversationId == conversationId
                );

            if (!supportTasks.Any())
            {
                throw new NotFoundException("No SupportTask Found");
            }
            return supportTasks;
        }

        public async Task<bool> CompleteTaskAsync(Guid taskId)
        {
            var repo = _unitOfWork.GetRepository<SupportTask>();
            var existSupportTask = await repo.GetByIdAsync(taskId);

            if (existSupportTask == null)
                throw new NotFoundException("No SupportTask found");

            if (existSupportTask.Status == SupportTaskStatus.Done)
                throw new BadRequestException("Task already completed");

            var now = DateTime.UtcNow;

           
            var handleTime = existSupportTask.CreatedAt.HasValue
                ? (int)(now - existSupportTask.CreatedAt.Value).TotalSeconds
                : 0;

            existSupportTask.Status = SupportTaskStatus.Done;
            existSupportTask.CompleteDate = now;

            await _unitOfWork.CommitAsync();

           
            if (existSupportTask.CurrentAssignedStaffId.HasValue)
            {
                await _staffPerformanceService.UpdatePerformanceOnTaskCompleteAsync(
                    existSupportTask.CurrentAssignedStaffId.Value,
                    handleTime
                );
            }

            return true;
        }

    }
}
