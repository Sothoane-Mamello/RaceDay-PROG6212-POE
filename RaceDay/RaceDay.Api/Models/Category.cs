
using System;
using System.Collections.Generic;

namespace RaceDay.Api.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string CategoryType { get; set; } = string.Empty;

        public int? MinAge { get; set; }

        public int? MaxAge { get; set; }

        public decimal? DistanceKm { get; set; }

        public string? Description { get; set; }

        public Event Event { get; set; } = null!;

        public ICollection<EventEnrolment> EventEnrolments { get; set; }
            = new List<EventEnrolment>();
    }
}
