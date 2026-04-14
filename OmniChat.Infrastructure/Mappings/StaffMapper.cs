using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.Staff;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
using OmniChat.Infrastructure.Dtos.Responses.SupportTask;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class StaffMapper  : Profile
{
    public StaffMapper()
    {

        CreateMap<CreateStaffRequest, Staff>().ForMember(dest => dest.StaffIntentTypes, opt => opt.Ignore());
        CreateMap<UpdateStaffRequest, Staff>()
            .ForMember(dest => dest.StaffIntentTypes, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Staff, GetStaffsResponse>()
            .ForMember(dest => dest.StaffIntentTypes,
                opt => opt.MapFrom(src => src.StaffIntentTypes))
            .ForMember(dest => dest.AvatarUrl,
               opt => opt.MapFrom(src => src.Account != null ? src.Account.AvatarUrl : null))
            ;

        CreateMap<SupportTask, StaffSupportTaskResponse>()
             .ForMember(dest => dest.IntentTypeName,
                 opt => opt.MapFrom(src => src.IntentType != null ? src.IntentType.TypeName : null))
             .ForMember(dest => dest.CustomerName,
                 opt => opt.MapFrom(src => (src.SupportConversation != null && src.SupportConversation.Staff != null)
                     ? src.SupportConversation.Staff.Name : "N/A"))
             .ForMember(dest => dest.CompletedAt,
                 opt => opt.MapFrom(src => src.CompleteDate))
             .ForMember(dest => dest.Status,
                 opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
