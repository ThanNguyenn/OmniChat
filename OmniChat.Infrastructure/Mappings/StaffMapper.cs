using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.Staff;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
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

        CreateMap<CreateStaffRequest, Staff>();
        CreateMap<UpdateStaffRequest, Staff>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Staff, GetStaffsResponse>();
    }
}
