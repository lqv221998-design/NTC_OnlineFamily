using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace NTC.Core.DTOs
{
    public class FamilyUploadDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [Range(2020, 2030)]
        public int RevitVersion { get; set; }

        [Required]
        public string FilePath { get; set; }
    }
}
