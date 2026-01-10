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
        public string Id { get; set; }          // Instagram Business ID
        public long Time { get; set; }
        public List<InstagramChange> Changes { get; set; }
    }
}
