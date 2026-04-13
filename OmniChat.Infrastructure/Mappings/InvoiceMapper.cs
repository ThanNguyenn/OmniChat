using AutoMapper;
using OmniChat.Infrastructure.Dtos.Responses.Invoice;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class InvoiceMapper : Profile
{
    public InvoiceMapper()
    {
        CreateMap<Invoice, GetInvoiceResponse>().ForMember(dest => dest.CustomerName,
                 opt => opt.MapFrom(src => src.CustomerProfile.CustomerName)).ForMember(dest => dest.CustomerPhoneNumber,
                 opt => opt.MapFrom(src => src.CustomerProfile.PhoneNumber)).ForMember(dest => dest.CustomerEmail,
                 opt => opt.MapFrom(src => src.CustomerProfile.Email)).ForMember(dest => dest.CustomerAddress,
                 opt => opt.MapFrom(src => src.CustomerProfile.Address));
        CreateMap<Invoice, GetInvoicesResponse>().ForMember(dest => dest.CustomerName,
                 opt => opt.MapFrom(src => src.CustomerProfile.CustomerName)).ForMember(dest => dest.CustomerPhoneNumber,
                 opt => opt.MapFrom(src => src.CustomerProfile.PhoneNumber)).ForMember(dest => dest.CustomerEmail,
                 opt => opt.MapFrom(src => src.CustomerProfile.Email)).ForMember(dest => dest.CustomerAddress,
                 opt => opt.MapFrom(src => src.CustomerProfile.Address));
    }
}
