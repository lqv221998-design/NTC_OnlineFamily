using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace NTC.Core.Models
{
    public abstract class BaseModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
