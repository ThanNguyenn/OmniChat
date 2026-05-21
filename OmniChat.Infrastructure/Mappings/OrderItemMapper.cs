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
             (src.PostSaleItem != null &&
              (src.PostSaleItem.PostSaleRequest.Type == PostSaleRequestType.Return && src.PostSaleItem.PostSaleRequest.Status == PostSaleRequestStatus.Approved )
                 ? src.PostSaleItem.Quantity
                 : 0)
         ))

     .ForMember(dest => dest.ItemsPrice,
         opt => opt.MapFrom(src =>
             src.ProductBatch.Product.Price *
             (
                 src.Quantity -
                 (src.PostSaleItem != null &&
                  src.PostSaleItem.PostSaleRequest.Type == PostSaleRequestType.Return
                     ? src.PostSaleItem.Quantity
                     : 0)
             )
         ));
    }
}
