using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.Staff;
using OmniChat.Infrastructure.Dtos.Responses.IntentType;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class IntentTypeMapper : Profile
{
    public IntentTypeMapper()
    {
        CreateMap<IntentType, GetsIntentTypeResponse>();

        // Create
        CreateMap<StaffIntentType, GetStaffIntentTypeResponse >().ForMember(dest => dest.IntentTypeName, opt => opt.MapFrom(src => src.IntentType.TypeName)).ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IntentType.Id));
        CreateMap<AssignStaffToIntentTypeRequest, StaffIntentType>();
    }

}
