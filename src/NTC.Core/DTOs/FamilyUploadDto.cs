using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace NTC.Core.DTOs
{
    public class FamilyUploadDto
    {
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("category")]
        public string Category { get; set; }

        [Required]
        [Range(2020, 2030)]
        [JsonProperty("revit_version")]
        public int RevitVersion { get; set; }

        [Required]
        public string FilePath { get; set; } // Local file path, not sent to DB directly
    }
}
