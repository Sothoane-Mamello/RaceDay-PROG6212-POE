
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/results")]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public ResultsController(RaceDayDbContext db)
        {
            _db = db;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        // POST: api/results
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> RecordResult(ResultDto dto)
        {
            var enrolment = await _db.EventEnrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == dto.EnrolmentId);

            if (enrolment == null)
                return NotFound(new
                {
                    message = "Enrolment not found."
                });

            if (enrolment.Event.OrganiserId != CurrentUserId)
                return Forbid();

            if (enrolment.Status != "Confirmed")
                return Conflict(new
                {
                    message = "Enrolment is not confirmed."
                });

            bool exists = await _db.Results
                .AnyAsync(r => r.EnrolmentId == dto.EnrolmentId);

            if (exists)
                return Conflict(new
                {
                    message = "Result already recorded."
                });

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTimeSeconds = dto.FinishTimeSeconds,
                FinishingPosition = dto.FinishingPosition,
                RecordedAt = DateTime.UtcNow
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Result recorded successfully.",
                result.ResultId,
                result.EnrolmentId,
                result.FinishTimeSeconds,
                result.FinishingPosition
            });
        }

        // GET: api/results/my
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> MyResults()
        {
            var results = await _db.Results
                .AsNoTracking()
                .Where(r =>
                    r.EventEnrolment.ParticipantId == CurrentUserId)
                .OrderBy(r => r.FinishingPosition)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    EventName = r.EventEnrolment.Event.Name,
                    CategoryName = r.EventEnrolment.Category.Name,
                    r.FinishTimeSeconds,
                    r.FinishingPosition,
                    r.RecordedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/results/event/2
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> EventResults(int eventId)
        {
            var raceEvent = await _db.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (raceEvent == null)
                return NotFound(new
                {
                    message = "Event not found."
                });

            if (raceEvent.OrganiserId != CurrentUserId)
                return Forbid();

            var results = await _db.Results
                .AsNoTracking()
                .Where(r =>
                    r.EventEnrolment.EventId == eventId)
                .OrderBy(r => r.FinishingPosition)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    ParticipantName =
                        r.EventEnrolment.Participant.User.FullName,
                    CategoryName =
                        r.EventEnrolment.Category.Name,
                    r.FinishTimeSeconds,
                    r.FinishingPosition
                })
                .ToListAsync();

            return Ok(results);
        }

        // PUT: api/results/1
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(
            int id, ResultDto dto)
        {
            var result = await _db.Results
                .Include(r => r.EventEnrolment)
                .ThenInclude(e => e.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound();

            if (result.EventEnrolment.Event.OrganiserId != CurrentUserId)
                return Forbid();

            if (result.EnrolmentId != dto.EnrolmentId)
                return BadRequest(new
                {
                    message = "Cannot change the result enrolment."
                });

            result.FinishTimeSeconds = dto.FinishTimeSeconds;
            result.FinishingPosition = dto.FinishingPosition;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Result updated successfully."
            });
        }
    }
}
