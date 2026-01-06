using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
using OmniChat.Infrastructure.Dtos.Responses.SupportStaffMessage;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class SupportStaffMessageMapper : Profile
    {
        public SupportStaffMessageMapper()
        {
            CreateMap<CreateSupportStaffMessageRequest, SupportStaffMessage>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => SupportStaffMessageStatus.Pending))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()))
            .ForMember(dest => dest.SupportConversation, opt => opt.Ignore())
            .ForMember(dest => dest.Staff, opt => opt.Ignore());

            CreateMap<SupportStaffMessage, CreateSupportStaffMessageResponse>();

        }
    }
}
