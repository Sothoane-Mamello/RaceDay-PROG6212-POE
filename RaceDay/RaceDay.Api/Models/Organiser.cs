
using System.Collections.Generic;

namespace RaceDay.Api.Models
{
    public class Organiser
    {
        public int OrganiserId { get; set; }

        public string? OrganisationName { get; set; }

        public string? PhoneNumber { get; set; }

        // Links the organiser to their user account
        public User User { get; set; } = null!;

        // Events created by this organiser
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
