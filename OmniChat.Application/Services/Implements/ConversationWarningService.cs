using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.WarningConversation;
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
    public class ConversationWarningService : BaseService<ConversationWarningService>, IConversationWarningService
    {


        public ConversationWarningService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<ConversationWarningService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<WarningDetailRepsone>> GetAllWarningsAsync(bool? isReviewed = null)
        {
            List<WarningDetailRepsone> respones = new List<WarningDetailRepsone>();

            var warningRepo =  _unitOfWork.GetRepository<ConversationWarning>();

            var warnings = await warningRepo
                .GetListAsync(
                    predicate: w => isReviewed == null || w.IsReviewed == isReviewed,
                    include: q => q.
                    Include(w => w.Staff)
                   .Include(w => w.Conversation).ThenInclude(c => c.CustomerProfile)
                );

            foreach (var item in warnings)
            {
                var response = new WarningDetailRepsone
                {
                    CustomerName = item.Conversation.CustomerProfile.CustomerName,
                    StaffName = item.Staff.Name,
                    CreateAt = item.CreatedAt,
                    WarningType = item.WarningType,
                    Reason = item.Reason
                };

                respones.Add(response);
            }

            return respones;

        }

        public async Task<WarningDetailRepsone> GetWarningByIdAsync(Guid id)
        {
            var warningRepo = _unitOfWork.GetRepository<ConversationWarning>();
            var warning = await warningRepo
                .SingleOrDefaultAsync(
                    predicate: w => w.Id == id,
                    include: q => q.
                       Include(w => w.Staff)
                      .Include(w => w.Conversation).ThenInclude(c => c.CustomerProfile)
                );

            if (warning == null)
                throw new NotFoundException($"Warning {id} not found");

            var response = new WarningDetailRepsone
            {
                CustomerName = warning.Conversation.CustomerProfile.CustomerName,
                StaffName = warning.Staff.Name,
                CreateAt = warning.CreatedAt,
                WarningType = warning.WarningType,
                Reason = warning.Reason
            };

            return response;
        }
    }
}
