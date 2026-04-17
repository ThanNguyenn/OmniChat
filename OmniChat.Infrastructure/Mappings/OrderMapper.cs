using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Dtos.Requests.Product;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class OrderMapper : Profile
{
    public OrderMapper()
    {
        CreateMap<CreateOrderRequest, Order>()
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore());
        CreateMap<UpdateOrderRequest, Order>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Order, GetAllOrdersResponse>().ForMember(dest => dest.CustomerName,
                 opt => opt.MapFrom(src => src.CustomerProfile.CustomerName));
        CreateMap<Order, GetOrderResponse>().ForMember(dest => dest.CustomerName,
                 opt => opt.MapFrom(src => src.CustomerProfile.CustomerName)).ForMember(dest => dest.CustomerPhoneNumber,
                 opt => opt.MapFrom(src => src.CustomerProfile.PhoneNumber)).ForMember(dest => dest.CustomerEmail,
                 opt => opt.MapFrom(src => src.CustomerProfile.Email)).ForMember(dest => dest.CustomerAddress,
                 opt => opt.MapFrom(src => src.CustomerProfile.Address)).ForMember(dest => dest.OrderItems,
                 opt => opt.MapFrom(src => src.OrderItems));
        CreateMap<Order, GetPostSaleOrderResponse>();

        CreateMap<Order, GetOrderForShipperResponse>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustomerProfile.CustomerName))
            .ForMember(dest => dest.CustomerPhoneNumber, opt => opt.MapFrom(src => src.CustomerProfile.PhoneNumber))
            .ForMember(dest => dest.CustomerAddress, opt => opt.MapFrom(src => src.CustomerProfile.Address))
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));
      ;
    }
}
