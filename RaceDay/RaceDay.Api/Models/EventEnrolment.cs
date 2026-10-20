
using System;

namespace RaceDay.Api.Models
{
    public class EventEnrolment
    {
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }

        public int EventId { get; set; }

        public int CategoryId { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Confirmed";

        public Participant Participant { get; set; } = null!;

        public Event Event { get; set; } = null!;

        public Category Category { get; set; } = null!;

        public Result? Result { get; set; }
    }
}
