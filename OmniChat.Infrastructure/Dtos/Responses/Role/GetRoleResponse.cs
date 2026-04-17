using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Role;

public class GetRoleResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; }
}
