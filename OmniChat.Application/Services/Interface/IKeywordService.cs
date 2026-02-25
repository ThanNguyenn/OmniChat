using OmniChat.Infrastructure.Dtos.Requests.Keyword;
using OmniChat.Infrastructure.Dtos.Responses.Keyword;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IKeywordService
{
    Task<GetKeywordResponse> GetKeywordAsync(Guid keywordId);
    Task<IEnumerable<GetAllKeywordsResponse>> GetAllKeywordsAsync();
    Task<bool> CreateKeywordAsync(CreateKeywordRequest createKeywordRequest);

    Task<bool> UpdateKeywordAsync(Guid keywordId, UpdateKeywordRequest updateKeywordRequest);

    Task<bool> DeleteKeywordAsync(Guid keywordId);
}
