using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Application.Webhooks.Instagram.InstagramMessage
{
    public class InstagramEntry
    {
        public string id { get; set; }          // Instagram Business ID
        public long time { get; set; }
        
        public List<InstagramMessage> messaging {  get; set; }
    }
}
