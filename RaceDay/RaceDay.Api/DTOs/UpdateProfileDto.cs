
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        [StringLength(30)]
        public string? PhoneNumber { get; set; }

        [StringLength(150)]
        public string? OrganisationName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(120)]
        public string? EmergencyContact { get; set; }
    }
}
