using OmniChat.Application.Webhooks.Instagram.InstagramMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Webhooks.Instagram
{
    public class InstagramChange
    {
        public string field { get; set; }       // "messages"
        public InstagramChangeValue value { get; set; }
    }
}
