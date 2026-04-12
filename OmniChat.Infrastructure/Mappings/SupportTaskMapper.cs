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

            CreateMap<SupportTask, ConversationTaskResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.IntentTypeName, opt => opt.MapFrom(src => src.IntentType != null ? src.IntentType.TypeName : string.Empty));
        }

    }
}
