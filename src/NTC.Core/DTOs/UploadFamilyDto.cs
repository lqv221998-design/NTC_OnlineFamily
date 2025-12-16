using System.ComponentModel.DataAnnotations;

namespace NTC.Core.DTOs
{
    public class UploadFamilyDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [Range(2020, 2030)]
        public int RevitVersion { get; set; }

        [Required]
        public string FilePath { get; set; } // Local path to .rfa file
    }
}
