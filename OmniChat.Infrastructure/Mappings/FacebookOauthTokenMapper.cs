using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.FacebookOauthToken;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class FacebookOauthTokenMapper : Profile
    {
        public FacebookOauthTokenMapper()
        {
            CreateMap<FacebookOauthTokenRequest, FacebookOathToken>()
          .ForMember(dest => dest.LastUpdateAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.AccessTokenExpiredDate, opt => opt.Ignore());
        }
    }
}
