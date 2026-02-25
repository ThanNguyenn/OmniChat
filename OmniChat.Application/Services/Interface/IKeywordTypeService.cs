using OmniChat.Infrastructure.Dtos.Requests.KeywordType;
using OmniChat.Infrastructure.Dtos.Responses.KeywordType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IKeywordTypeService
{
    Task<bool> CreateKeywordTypeAsync(CreateKeywordTypeResquest keywordTypeResquest);
    Task<bool> DeleteKeywordTypeAsync(Guid keywordTypeId);

    Task<GetKeywordTypeResponse> GetKeywordTypeAsync(Guid keywordTypeId);

    Task<IEnumerable<GetKeywordTypesResponse>> GetAllKeywordTypesAsync();
}
