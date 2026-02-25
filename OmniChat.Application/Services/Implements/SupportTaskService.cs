using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
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
        public SupportTaskService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<SupportTaskService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
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
    }
}
