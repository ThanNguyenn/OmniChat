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

        [JsonPropertyName("user_is_follower")]
        public bool UserIsFollower { get; set; }

        [JsonPropertyName("user_last_interaction_date")]
        public string LastInteractionDate { get; set; }

        [JsonPropertyName("shared_info")]
        public ZaloSharedInfo SharedInfo { get; set; }
    }
}
