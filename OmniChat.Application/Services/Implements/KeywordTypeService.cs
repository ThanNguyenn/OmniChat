using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.KeywordType;
using OmniChat.Infrastructure.Dtos.Responses.KeywordType;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;

namespace OmniChat.Application.Services.Implements;

public class KeywordTypeService : BaseService<KeywordTypeService>, IKeywordTypeService
{

    public KeywordTypeService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<KeywordTypeService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<bool> CreateKeywordTypeAsync(CreateKeywordTypeResquest createKeywordTypeResquest)
    {
        var keywordTypeRepo = _unitOfWork.GetRepository<IntentType>();
        var existingKeywordType = await keywordTypeRepo.SingleOrDefaultAsync(predicate: kt => kt.TypeName == createKeywordTypeResquest.TypeName);
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var keywordType = _mapper.Map<IntentType>(createKeywordTypeResquest);
            await keywordTypeRepo.InsertAsync(keywordType);
        });
        return true;
    }

    public async Task<bool> DeleteKeywordTypeAsync(Guid keywordTypeId)
    {
        var keywordTypeRepo = _unitOfWork.GetRepository<IntentType>();
        var existingKeywordType = await keywordTypeRepo.SingleOrDefaultAsync(predicate: kt => kt.Id == keywordTypeId) ?? throw new NotFoundException($"Không tìm thấy loại từ khóa");
        existingKeywordType.IsActive = false;

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            keywordTypeRepo.Update(existingKeywordType);
        });
        return true;
    }

    public async Task<IEnumerable<GetKeywordTypesResponse>> GetAllKeywordTypesAsync()
    {
        var keywordTypeRepo = _unitOfWork.GetRepository<IntentType>();
        var response = _mapper.Map<IEnumerable<GetKeywordTypesResponse>>(await keywordTypeRepo.GetListAsync(predicate: kt => kt.IsActive != false));
        return response;
    }
    public async Task<GetKeywordTypeResponse> GetKeywordTypeAsync(Guid keywordTypeId)
    {
        var keywordTypeRepo = _unitOfWork.GetRepository<IntentType>();
        var existingKeywordType = await keywordTypeRepo.SingleOrDefaultAsync(predicate: kt => kt.Id == keywordTypeId && kt.IsActive != false) ?? throw new NotFoundException($"Không tìm thấy loại từ khóa");
        var response = _mapper.Map<GetKeywordTypeResponse>( existingKeywordType);
        return response;
    }
}
