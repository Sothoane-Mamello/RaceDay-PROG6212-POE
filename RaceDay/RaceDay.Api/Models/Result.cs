
using System;

namespace RaceDay.Api.Models
{
    public class Result
    {
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        public int FinishTimeSeconds { get; set; }

        public int FinishingPosition { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        public EventEnrolment EventEnrolment { get; set; } = null!;
    }
}
