
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class EventDto
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(250)]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 9999.99)]
        public decimal DistanceKm { get; set; }

        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$")]
        public string EventType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? BannerImageUrl { get; set; }
    }
}
