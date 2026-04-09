using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.ChatTemplate
{
    public class ChatTemplateRequest
    {
        public string Code { get; set; }
        public string Content { get; set; }
    }
}
