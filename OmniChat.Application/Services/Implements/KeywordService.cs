using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Keyword;
using OmniChat.Infrastructure.Dtos.Responses.Keyword;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class KeywordService : BaseService<KeywordService>, IKeywordService
{
    public KeywordService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<KeywordService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }
    public async Task<bool> CreateKeywordAsync(CreateKeywordRequest createKeywordRequest)
    {
        var keywordRepo = _unitOfWork.GetRepository<Keyword>();
        await _unitOfWork.ProcessInTransactionAsync(async () => {
            var keyword = _mapper.Map<Keyword>(createKeywordRequest);
            await keywordRepo.InsertAsync(keyword);
        });
        return true;

    }
    public async Task<bool> DeleteKeywordAsync(Guid keywordId)
    {
        var keywordRepo = _unitOfWork.GetRepository<Keyword>();
        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var existingKeyword = await keywordRepo.SingleOrDefaultAsync(predicate: k => k.Id == keywordId && k.IsDeleted != true) ?? throw new NotFoundException($"Keyword {keywordId} not found");
                existingKeyword.IsDeleted = true;
                keywordRepo.Update(existingKeyword);
            return true;
        });
    }
    public Task<IEnumerable<GetAllKeywordsResponse>> GetAllKeywordsAsync()
    {
        var keywordRepo = _unitOfWork.GetRepository<Keyword>();
        return _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var keywords = await keywordRepo.GetListAsync(predicate: k => k.IsDeleted != true);
            return _mapper.Map<IEnumerable<GetAllKeywordsResponse>>(keywords);
        });
    }
    public Task<GetKeywordResponse> GetKeywordAsync(Guid keywordId)
    {
        var keywordRepo = _unitOfWork.GetRepository<Keyword>();
        return _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var keyword = await keywordRepo.SingleOrDefaultAsync(predicate: k => k.Id == keywordId && k.IsDeleted != true) ?? throw new NotFoundException($"Keyword {keywordId} not found");
            return _mapper.Map<GetKeywordResponse>(keyword);
        });
    }
    public Task<bool> UpdateKeywordAsync(Guid keywordId, UpdateKeywordRequest updateKeywordRequest)
    {
        var keywordRepo = _unitOfWork.GetRepository<Keyword>();
        return _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var existingKeyword = await keywordRepo.SingleOrDefaultAsync(predicate: k => k.Id == keywordId && k.IsDeleted != true) ?? throw new NotFoundException($"Keyword {keywordId} not found");
            _mapper.Map(updateKeywordRequest, existingKeyword);
            keywordRepo.Update(existingKeyword);
            return true;
        });
    }
}
