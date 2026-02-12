using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.Product;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Models;

namespace OmniChat.Infrastructure.Mappings;

public class ProductMapper : Profile
{
    public ProductMapper()
    {
        CreateMap<CreateProductRequest, Product>();
        CreateMap<UpdateProductRequest, Product>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Product, GetAllProductsResponse>();
        CreateMap<Product, GetProductResponse>();       
    }
}
