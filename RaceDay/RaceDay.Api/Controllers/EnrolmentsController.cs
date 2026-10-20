
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
    [Route("api/enrolments")]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public EnrolmentsController(RaceDayDbContext db)
        {
            _db = db;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        // POST: api/enrolments
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Create(EnrolmentDto dto)
        {
            var participantExists = await _db.Participants
                .AnyAsync(p => p.ParticipantId == CurrentUserId);

            if (!participantExists)
                return Forbid();

            var raceEvent = await _db.Events
                .FirstOrDefaultAsync(e => e.EventId == dto.EventId);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            var category = await _db.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == dto.CategoryId &&
                    c.EventId == dto.EventId);

            if (category == null)
                return BadRequest(new
                {
                    message = "Category does not belong to this event."
                });

            bool alreadyEnrolled = await _db.EventEnrolments
                .AnyAsync(e =>
                    e.ParticipantId == CurrentUserId &&
                    e.EventId == dto.EventId);

            if (alreadyEnrolled)
                return Conflict(new
                {
                    message = "You are already enrolled in this event."
                });

            var enrolment = new EventEnrolment
            {
                ParticipantId = CurrentUserId,
                EventId = dto.EventId,
                CategoryId = dto.CategoryId,
                Status = "Confirmed",
                EnrolledAt = DateTime.UtcNow
            };

            _db.EventEnrolments.Add(enrolment);
            await _db.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Enrolment successful.",
                enrolment.EnrolmentId,
                enrolment.EventId,
                enrolment.CategoryId,
                enrolment.Status
            });
        }

        // GET: api/enrolments/my
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> MyEnrolments()
        {
            var enrolments = await _db.EventEnrolments
                .AsNoTracking()
                .Where(e => e.ParticipantId == CurrentUserId)
                .OrderByDescending(e => e.EnrolledAt)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.EventId,
                    EventName = e.Event.Name,
                    e.CategoryId,
                    CategoryName = e.Category.Name,
                    e.Status,
                    e.EnrolledAt
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/enrolments/event/2
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> EventEnrolments(int eventId)
        {
            var raceEvent = await _db.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.OrganiserId != CurrentUserId)
                return Forbid();

            var enrolments = await _db.EventEnrolments
                .AsNoTracking()
                .Where(e => e.EventId == eventId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    ParticipantName = e.Participant.User.FullName,
                    e.EventId,
                    e.CategoryId,
                    CategoryName = e.Category.Name,
                    e.Status,
                    e.EnrolledAt
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // DELETE: api/enrolments/1
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Cancel(int id)
        {
            var enrolment = await _db.EventEnrolments
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            if (enrolment.ParticipantId != CurrentUserId)
                return Forbid();

            if (enrolment.Status == "Cancelled")
                return Conflict(new
                {
                    message = "Enrolment already cancelled."
                });

            bool hasResult = await _db.Results
                .AnyAsync(r => r.EnrolmentId == id);

            if (hasResult)
                return Conflict(new
                {
                    message = "Cannot cancel an enrolment with results."
                });

            enrolment.Status = "Cancelled";
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
