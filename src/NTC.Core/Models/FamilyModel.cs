using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NTC.Core.Models
{
    public class FamilyModel : BaseModel
    {
        [Required]
        [StringLength(255)]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("category")]
        public string Category { get; set; }

        [Required]
        [JsonProperty("file_url")]
        public string Url { get; set; }

        [Required]
        [Range(2020, 2030)]
        [JsonProperty("revit_version")]
        public int RevitVersion { get; set; }

        [JsonProperty("status")]
        public FamilyStatus Status { get; set; } = FamilyStatus.Pending;

        [JsonProperty("parameters")]
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
