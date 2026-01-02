using AutoMapper;
using OmniChat.Infrastructure.Dtos.Responses.Provider;
using OmniChat.Infrastructure.Dtos.Requests.Provider;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class ProviderMapper : Profile
    {
        public ProviderMapper()
        {
            CreateMap<CreateProviderRequest, Provider>()
             .ForMember(dest => dest.Id,
                 opt => opt.MapFrom(_ => Guid.NewGuid()))
             .ForMember(dest => dest.ProviderName,
                 opt => opt.MapFrom(src => src.ProviderName))
             .ForMember(dest => dest.CreateDate,
                 opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Provider, CreateProviderResponse>();

        }
    }
}
