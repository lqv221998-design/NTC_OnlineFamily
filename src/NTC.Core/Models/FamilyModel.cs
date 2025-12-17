using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NTC.Core.Models
{
    public class FamilyModel : BaseModel
    {
        [Required(ErrorMessage = "Family Name is mandatory.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 255 chars.")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category is mandatory.")]
        [JsonProperty("category")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Revit Version is mandatory.")]
        [Range(2020, 2030, ErrorMessage = "Only Revit versions 2020-2030 are supported.")]
        [JsonProperty("revit_version")]
        public int RevitVersion { get; set; }

        [Required(ErrorMessage = "File URL cannot be empty.")]
        [JsonProperty("file_url")] // Mapped to Supabase column: file_url
        public string Url { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("status")]
        // Enum ensures strict data domain values (Pending/Approved)
        public FamilyStatus Status { get; set; } = FamilyStatus.Pending;

        [JsonProperty("parameters")]
        // Maps to JSONB column for flexible metadata
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        [JsonProperty("created_by")]
        public Guid? CreatedBy { get; set; }
    }
}
