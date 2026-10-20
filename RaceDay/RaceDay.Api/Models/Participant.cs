
using System;
using System.Collections.Generic;

namespace RaceDay.Api.Models
{
    public class Participant
    {
        public int ParticipantId { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? PhoneNumber { get; set; }

        public string? EmergencyContact { get; set; }

        // Links the participant to their user account
        public User User { get; set; } = null!;

        // Events the participant has enrolled in
        public ICollection<EventEnrolment> EventEnrolments { get; set; } = new List<EventEnrolment>();
    }
}
