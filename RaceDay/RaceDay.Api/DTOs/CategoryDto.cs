
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class CategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Age|Distance)$")]
        public string CategoryType { get; set; } = string.Empty;

        public int? MinAge { get; set; }

        public int? MaxAge { get; set; }

        public decimal? DistanceKm { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }
    }
}
