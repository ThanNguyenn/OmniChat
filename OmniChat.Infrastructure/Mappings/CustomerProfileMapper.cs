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
            .ForMember(x => x.CreateDate, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<CustomerProfile, GetCustomerProfileResponse>()
                .ForMember(dest => dest.CustomerDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.TotalOrder, opt => opt.MapFrom(src => src.Orders != null ? src.Orders.Count : 0))
                .ForMember(dest => dest.TotalPayment, opt => opt.MapFrom(src =>
                    src.Invoices != null ? src.Invoices.Sum(p => (double)(p.Total - (p.DeductedAmount))) : 0))
                .ForMember(dest => dest.getWalletResponse, opt => opt.Ignore());

            CreateMap<CustomerProfile, CustomerDetailResponse>()
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.BecomeCustomerDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.TotalOrder, opt => opt.MapFrom(src => src.Orders != null ? src.Orders.Count : 0))
                .ForMember(dest => dest.TotalPay, opt => opt.MapFrom(src =>
                    src.Invoices != null ? src.Invoices.Sum(p => (double)(p.Total - (p.DeductedAmount))) : 0))
                .ForMember(dest => dest.ProviderName, opt => opt.Ignore())
                .ForMember(dest => dest.TimeStartSupport, opt => opt.Ignore())
                .ForMember(dest => dest.getWalletResponse, opt => opt.Ignore());
        }
    }
}