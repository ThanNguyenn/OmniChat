using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.PostSaleRequestItem;
using OmniChat.Infrastructure.Dtos.Responses.PostSaleRequestItem;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class PostSaleRequestItemMapper : Profile
{
    public PostSaleRequestItemMapper()
    {
        CreateMap<PostSaleItem, GetPostSaleItemsResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.OrderItem.ProductBatch.Product.Name))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));

        CreateMap<CreatePostSaleRequestItemRequest, PostSaleItem>();
    }

}
