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
        CreateMap<CreateOrderRequest, Order>();
        CreateMap<UpdateOrderRequest, Order>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Order, GetAllOrdersResponse>();
        CreateMap<Order, GetOrderResponse>();
    }
}
