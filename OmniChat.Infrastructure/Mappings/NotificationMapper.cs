using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.Notification;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class NotificationMapper : Profile
    {
        public NotificationMapper()
        {
            CreateMap<NotificationRequest, Notification>()
                        .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                        .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false));
        }
    }
}
