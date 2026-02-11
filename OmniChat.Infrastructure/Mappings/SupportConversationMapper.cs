using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.SupportConversation;
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

        }
    }
}
