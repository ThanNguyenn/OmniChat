using AutoMapper;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatch;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class ProductBatchMapper: Profile
{
    public ProductBatchMapper()
    {
        CreateMap<ProductBatch, GetProductBatchesResponse>();
    }
}
