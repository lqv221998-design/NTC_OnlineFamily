using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NTC.Core.Models
{
    public class FamilyModel : BaseModel
    {
        [Required(ErrorMessage = "Family Name is required.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 255 characters.")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("category")]
        public string Category { get; set; }

        [Required]
        [Range(2020, 2030, ErrorMessage = "Revit Version must be between 2020 and 2030.")]
        [JsonProperty("revit_version")]
        public int RevitVersion { get; set; }

        [Required(ErrorMessage = "File URL is missing.")]
        [JsonProperty("file_url")]
        public string FileUrl { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("parameters")]
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        [JsonProperty("status")]
        public FamilyStatus Status { get; set; } = FamilyStatus.Pending;

        [JsonProperty("created_by")]
        public Guid? CreatedBy { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
