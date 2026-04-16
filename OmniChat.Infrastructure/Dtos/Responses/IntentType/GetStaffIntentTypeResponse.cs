using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.IntentType;

public class GetStaffIntentTypeResponse
{
    public Guid Id { get; set; }
    public string IntentTypeName { get; set; }
}
