using AutoMapper;
using OmniChat.Infrastructure.Dtos.Responses.SupportTask;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class SupportTaskMapper : Profile
    {
        public SupportTaskMapper()
        {
            CreateMap<SupportTask, SupportTasksResponse>()
                .ForMember(dest => dest.IntentTypeName, opt => opt.MapFrom(src => src.IntentType.TypeName));
        }

    }
}
