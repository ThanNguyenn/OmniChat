using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.CustomerProfile
{
    public class MergeCustomerProfileRequest
    {
        public Guid SourceCustomerId { get; set; }   // profile mới tạo (FB/IG)
        public Guid TargetCustomerId { get; set; }   // profile chuẩn
    }
}
