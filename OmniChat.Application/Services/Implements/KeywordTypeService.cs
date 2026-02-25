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
    public class KeywordTypeService : BaseService<KeywordTypeService>, IKeywordTypeService
    {
        public KeywordTypeService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<KeywordTypeService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<KeywordTypes> GetKeywordTypeByIdAsync(Guid keywordTypeId)
        {
            var repo = _unitOfWork.GetRepository<KeywordTypes>();

            var existKeywordType = await repo.SingleOrDefaultAsync(predicate: x => x.Id == keywordTypeId);

            if(existKeywordType == null)
            {
                throw new NotFoundException("No KeywordType Found");
            }

            return existKeywordType;
        }
    }
}
