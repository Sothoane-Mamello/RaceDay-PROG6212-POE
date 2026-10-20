
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class ResultDto
    {
        [Range(1, int.MaxValue)]
        public int EnrolmentId { get; set; }

        [Range(1, int.MaxValue)]
        public int FinishTimeSeconds { get; set; }

        [Range(1, int.MaxValue)]
        public int FinishingPosition { get; set; }
    }
}
