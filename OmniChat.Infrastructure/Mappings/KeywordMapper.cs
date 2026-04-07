using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.Keyword;
using OmniChat.Infrastructure.Dtos.Responses.Keyword;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class KeywordMapper : Profile
{
    public KeywordMapper()
    {
        CreateMap<CreateKeywordRequest, Keyword>();

        CreateMap<UpdateKeywordRequest, Keyword>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


        CreateMap<Keyword, GetAllKeywordsResponse>().ForMember(dest => dest.IntentTypeName, opt => opt.MapFrom(src => src.IntentType.TypeName));

        CreateMap<Keyword, GetKeywordResponse>().ForMember(dest => dest.IntentTypeName, opt => opt.MapFrom(src => src.IntentType.TypeName));
    }
}
