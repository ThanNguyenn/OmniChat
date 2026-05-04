using OmniChat.Infrastructure.Dtos.Responses.IntentType;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Staff;

public class GetStaffsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public Guid RoleId { get; set; }
    public string RoleName { get; set; }

    public StaffStatus Status { get; set; }

    //staff performance metrics

    public IEnumerable<GetStaffIntentTypeResponse> StaffIntentTypes { get; set; }
}
