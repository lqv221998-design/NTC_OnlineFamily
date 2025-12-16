using Newtonsoft.Json;
using System;

namespace NTC.Core.Models
{
    public class UserProfile : BaseModel
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("role")]
        public AppRole Role { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
