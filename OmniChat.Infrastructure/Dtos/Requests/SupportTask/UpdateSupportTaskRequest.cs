using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.SupportTask
{
    public class UpdateSupportTaskRequest
    {
        public Guid NewIntentTypeId { get; set; }
    }
}
