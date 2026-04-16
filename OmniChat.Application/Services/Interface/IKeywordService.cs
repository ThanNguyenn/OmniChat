using OmniChat.Infrastructure.Dtos.Requests.Keyword;
using OmniChat.Infrastructure.Dtos.Responses.Intent;
using OmniChat.Infrastructure.Dtos.Responses.Keyword;
using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IKeywordService
{
    Task<GetKeywordResponse> GetKeywordAsync(Guid keywordId);
    Task<PagingResponse<GetAllKeywordsResponse>> GetAllKeywordsAsync(Guid? intentTypeId, string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);
    Task<bool> CreateKeywordAsync(CreateKeywordRequest createKeywordRequest);

    Task<bool> UpdateKeywordAsync(Guid keywordId, UpdateKeywordRequest updateKeywordRequest);

    Task<bool> DeleteKeywordAsync(Guid keywordId);

    Task<PredictResponse> AnalyzeMessageWithKeywordsAsync(string message);

    Task<IReadOnlyList<NlpToken>> AnalyzeTextAsync(string message);

}
