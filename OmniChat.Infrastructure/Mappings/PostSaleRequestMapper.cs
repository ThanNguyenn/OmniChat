using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.PostSaleRequest;
using OmniChat.Infrastructure.Dtos.Responses.PostSaleRequest;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class PostSaleRequestMapper : Profile
{
    public PostSaleRequestMapper()
    {
        CreateMap<PostSaleRequest, GetPostSaleRequestsResponse>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CustomerName))
            .ForMember(dest => dest.PresentByStaffName, opt => opt.MapFrom(src => src.PresentByStaff.Name));

        CreateMap<PostSaleRequest, GetPostSaleRequestByIdResponse>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CustomerName))
            .ForMember(dest => dest.PresentByStaffName, opt => opt.MapFrom(src => src.PresentByStaff.Name));

        CreateMap<CreatePostSaleRequestRequest, PostSaleRequest>();
    }
}
