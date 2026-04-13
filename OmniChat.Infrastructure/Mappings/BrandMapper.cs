using AutoMapper;
using OmniChat.Infrastructure.Dtos.Responses.Brand;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class BrandMapper : Profile
{
    public BrandMapper()
    {
        CreateMap<Brand, GetAllBrandsResponse>();
    }
}
