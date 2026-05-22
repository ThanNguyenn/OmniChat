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

            .ForMember(dest => dest.Quantity,
                opt => opt.MapFrom(src =>
                    src.Quantity -
                    src.PostSaleItem
                        .Where(x =>
                            x.PostSaleRequest.Type == PostSaleRequestType.Return &&
                            x.PostSaleRequest.Status == PostSaleRequestStatus.Approved)
                        .Sum(x => x.Quantity)
                ))

            .ForMember(dest => dest.ItemsPrice,
                opt => opt.MapFrom(src =>
                    src.ProductBatch.Product.Price *
                    (
                        src.Quantity -
                        src.PostSaleItem
                            .Where(x =>
                                x.PostSaleRequest.Type == PostSaleRequestType.Return &&
                                x.PostSaleRequest.Status == PostSaleRequestStatus.Approved)
                            .Sum(x => x.Quantity)
                    )
                ));
    }
}
