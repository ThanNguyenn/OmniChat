using OmniChat.Infrastructure.Extensions;
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
        [JsonConverter(typeof(StringOrNumberConverter))]
        public string Phone { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }
    }
}
