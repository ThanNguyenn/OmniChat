using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.ClaimType;
using OmniChat.Infrastructure.Dtos.Responses.ClaimType;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class ClaimTypeMapper : Profile
    {
        public ClaimTypeMapper()
        {
            CreateMap<ClaimTypeRequest, ClaimType>();

            CreateMap<ClaimType, GetClaimTypeResponse>();
        }
    }
}
