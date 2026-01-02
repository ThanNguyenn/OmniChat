using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class CustomerProfileMapper : Profile
    {
        public CustomerProfileMapper()
        {
            CreateMap<CreateCustomerProfileRequest, CustomerProfile>()
            .ForMember(x => x.Id, opt => opt.MapFrom(_ => Guid.NewGuid()));

            CreateMap<CustomerProfile, CreateCustomerProfileResponse>();
        }
    }
}
