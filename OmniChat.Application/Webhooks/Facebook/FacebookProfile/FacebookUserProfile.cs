using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Application.Webhooks.Facebook.FacebookProfile
{
    public class FacebookUserProfile
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("profile_pic")]
        public string ProfilePic { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }
    }
}
