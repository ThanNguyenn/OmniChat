using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.IntentType;
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
    public class IntentTypeService : BaseService<IntentTypeService>, IIntentTypeService
    {
        public IntentTypeService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<IntentTypeService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<GetsIntentTypeResponse>> GetIntentTypesAsync()
        {
            var repo = _unitOfWork.GetRepository<IntentType>();

            var intentTypes = await repo.GetListAsync(predicate: it => it.IsActive == true, orderBy: q => q.OrderByDescending(q => q.CreateDate));

            return _mapper.Map<IEnumerable<GetsIntentTypeResponse>>(intentTypes);
        }
    }
}
