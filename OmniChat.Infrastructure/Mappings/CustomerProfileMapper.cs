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
                .ForMember(x => x.CreateDate,
                opt => opt.MapFrom(_ => DateTime.UtcNow)
                );

            CreateMap<CustomerProfile, GetCustomerProfileResponse>()
         .ForMember(
             dest => dest.TotalOrder,
             opt => opt.MapFrom(src => src.Orders.Count)
         )
         .ForMember(
             dest => dest.TotalPayment,
             opt => opt.MapFrom(src => src.Payments.Sum(p => p.Total))
         )
            .ForMember(
            dest => dest.CustomerDate,
            opt => opt.MapFrom(src => DateTime.Parse(src.CreateDate))
        );
        }
    }
}
