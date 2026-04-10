using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.OrderItem;
using OmniChat.Infrastructure.Dtos.Responses.OrderItem;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class OrderItemMapper : Profile
{
    public OrderItemMapper()
    {
        CreateMap<AddOrderItemRequest, OrderItem>();

        CreateMap<OrderItem, GetOrderItemResponse>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.ProductBatch.Product.Name))
            .ForMember(dest => dest.ItemsPrice, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                dest.ItemsPrice = src.ProductBatch.Product.Price * dest.Quantity;
            });
    }
}
