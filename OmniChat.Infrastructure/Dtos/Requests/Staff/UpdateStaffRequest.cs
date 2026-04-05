using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Staff;

public class UpdateStaffRequest
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public IEnumerable<AssignStaffToIntentTypeRequest>? StaffIntentTypes { get; set; }
}
