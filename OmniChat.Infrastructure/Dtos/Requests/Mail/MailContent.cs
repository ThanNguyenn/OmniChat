using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Mail
{
    public class MailContent
    {
        public string To { get; set; } // Địa chỉ gửi đi 

        public string Subject { get; set; } // subject của email

        public string Body { get; set; } // nội dung của email 
    }
}
