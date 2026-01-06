using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Account;

public class CreateAccountRequest
{
    [Required]
    public Guid StaffId { get; set; }

    [Required]
    public Guid RoleId { get; set; }
}
