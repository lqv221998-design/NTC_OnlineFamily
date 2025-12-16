using Newtonsoft.Json;
using System;

namespace NTC.Core.Models
{
    public class DownloadRecord : BaseModel
    {
        [JsonProperty("user_id")]
        public Guid? UserId { get; set; }

        [JsonProperty("family_id")]
        public Guid FamilyId { get; set; }

        [JsonProperty("downloaded_at")]
        public DateTime DownloadedAt { get; set; }
    }
}
