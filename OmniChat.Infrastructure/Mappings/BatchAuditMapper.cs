using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.ProductBatchAudit;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatchAudit;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class BatchAuditMapper : Profile
{

    public BatchAuditMapper()
    {
        CreateMap<BatchAudit, GetAllAuditResponse>()
         .ForMember(dest => dest.StaffName,
             opt => opt.MapFrom(src => src.ActionBy != null ? src.ActionBy.Name : null));

        CreateMap<BatchAudit, GetDetailByBatchIdResponse>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.ProductBatch.Product.Name))

            .ForMember(dest => dest.BrandName,
                opt => opt.MapFrom(src => src.ProductBatch.Product.Brand.Name))

            .ForMember(dest => dest.VolumeML,
                opt => opt.MapFrom(src => src.ProductBatch.Product.VolumeMl))

            .ForMember(dest => dest.Price,
                opt => opt.MapFrom(src => src.ProductBatch.Product.Price))

            .ForMember(dest => dest.ProductCode,
                opt => opt.MapFrom(src => src.ProductBatch.Product.Code))

            .ForMember(dest => dest.PackagingType,
                opt => opt.MapFrom(src => src.ProductBatch.Product.ProductPackagingType))

            .ForMember(dest => dest.BatchCode,
                opt => opt.MapFrom(src => src.ProductBatch.Code))

            .ForMember(dest => dest.BatchCreateDate,
                opt => opt.MapFrom(src => src.ProductBatch.CreateDate))

            .ForMember(dest => dest.BatchQuantity,
                opt => opt.MapFrom(src => src.ProductBatch.Quantity))

            .ForMember(dest => dest.BatchExpiredDate,
                opt => opt.MapFrom(src =>
                    src.ProductBatch.ExpiryDate.HasValue
                        ? (src.ProductBatch.ExpiryDate.Value - DateTime.UtcNow).Days
                        : 0))

            .ForMember(dest => dest.StaffName,
                opt => opt.MapFrom(src =>
                    src.ActionBy != null ? src.ActionBy.Name : null));
        CreateMap<UpdateBatchAuditRequest, BatchAudit>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

}
