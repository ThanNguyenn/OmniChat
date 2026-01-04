using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Application.Webhooks.Zalo.ZaloProflie
{
    public class ZaloSharedInfo
    {
        [JsonPropertyName("phone")]
        public string Phone { get; set; }
    }
}
