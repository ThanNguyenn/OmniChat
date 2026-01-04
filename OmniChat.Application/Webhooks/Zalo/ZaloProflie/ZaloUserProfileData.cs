using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Application.Webhooks.Zalo.ZaloProflie
{
    public class ZaloUserProfileData
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("gender")]
        public int? Gender { get; set; }

        [JsonPropertyName("birth_date")]
        public string BirthDate { get; set; }

        [JsonPropertyName("shared_info")]
        public ZaloSharedInfo SharedInfo { get; set; }
    }
}
