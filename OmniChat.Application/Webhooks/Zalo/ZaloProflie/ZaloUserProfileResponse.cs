using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Application.Webhooks.Zalo.ZaloProflie
{
    public class ZaloUserProfileResponse
    {
        [JsonPropertyName("error")]
        public int Error { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public ZaloUserProfileData Data { get; set; }
    }

}
