using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.FeedBack;
using OmniChat.Infrastructure.Dtos.Responses.FeedBack;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class FeedBackMapper : Profile
    {
        public FeedBackMapper()
        {
            CreateMap<FeedBackRequest, FeedBack>()
                  .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.CustomerEmail))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
            .ForMember(dest => dest.StaffId, opt => opt.Ignore())
            .ForMember(dest => dest.SupportConversationId, opt => opt.Ignore())
            .ForMember(dest => dest.FormUrl, opt => opt.Ignore());

            CreateMap<FeedBack, FeedBackResponse>()
            .ForMember(dest => dest.StaffName,
                opt => opt.MapFrom(src => src.Staff != null ? src.Staff.Name : string.Empty));
        }
    }
}
