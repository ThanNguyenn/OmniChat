using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.SupportConversation;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class SupportConversationMapper : Profile
    {
        public SupportConversationMapper() {

            CreateMap<CreateSupportConversationRequest, SupportConversation>()
                 .ForMember(x => x.UpdateDate,
                opt => opt.MapFrom(_ => DateTime.UtcNow)
                );
            CreateMap<SupportConversation, SupportConversationDetailResponse>();

            CreateMap<SupportConversation, StaffConversationResponse>()
    .ForMember(dest => dest.ConversationId, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.customerName, opt => opt.MapFrom(src => src.CustomerName));
        }
    }
}
