using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.Claim;
using OmniChat.Infrastructure.Dtos.Responses.Claim;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class ClaimMapper : Profile
    {
        // Create
        public ClaimMapper() {

            // Create
            CreateMap<CreateClaimRequest, Claim>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => ClaimStatus.Pending))
                .ForMember(dest => dest.SubmitDate,
                           opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
                
            // Update
            CreateMap<UpdateClaimRequest, Claim>()
           .ForMember(dest => dest.Id, opt => opt.Ignore())
           .ForMember(dest => dest.Status, opt => opt.Ignore())
           .ForMember(dest => dest.SubmitDate, opt => opt.Ignore())
           .ForMember(dest => dest.StaffId, opt => opt.Ignore());

            // Entity → Response
            CreateMap<Claim, ClaimDetailResponse>()
            .ForMember(dest => dest.StaffName,
               opt => opt.MapFrom(src => src.Staff.Name)) 
            .ForMember(dest => dest.ClaimTypeName,
               opt => opt.MapFrom(src => src.ClaimType.TypeName));

            CreateMap<Claim, StaffClaimResponse>()
              .ForMember(dest => dest.ClaimType, opt => opt.MapFrom(src => src.ClaimType.TypeName));
        }
    }
}
