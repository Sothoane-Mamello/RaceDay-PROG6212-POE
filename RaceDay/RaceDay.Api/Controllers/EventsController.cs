
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
    [Route("api/events")]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        // GET: api/events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.DistanceKm,
                    e.EventType,
                    e.BannerImageUrl,
                    e.OrganiserId
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/5
        [HttpGet("{eventId:int}")]
        public async Task<IActionResult> GetEvent(int eventId)
        {
            var raceEvent = await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == eventId)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.DistanceKm,
                    e.EventType,
                    e.BannerImageUrl,
                    e.OrganiserId
                })
                .FirstOrDefaultAsync();

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            return Ok(raceEvent);
        }

        // POST: api/events
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(EventDto dto)
        {
            var organiserExists = await _context.Organisers
                .AnyAsync(o => o.OrganiserId == CurrentUserId);

            if (!organiserExists)
                return Forbid();

            var raceEvent = new Event
            {
                OrganiserId = CurrentUserId,
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location,
                DistanceKm = dto.DistanceKm,
                EventType = dto.EventType,
                BannerImageUrl = dto.BannerImageUrl
            };

            _context.Events.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { eventId = raceEvent.EventId },
                new
                {
                    raceEvent.EventId,
                    raceEvent.Name,
                    raceEvent.OrganiserId
                });
        }

        // PUT: api/events/5
        [HttpPut("{eventId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(
            int eventId, EventDto dto)
        {
            var raceEvent = await _context.Events
                .FindAsync(eventId);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.OrganiserId != CurrentUserId)
                return Forbid();

            raceEvent.Name = dto.Name;
            raceEvent.Description = dto.Description;
            raceEvent.EventDate = dto.EventDate;
            raceEvent.Location = dto.Location;
            raceEvent.DistanceKm = dto.DistanceKm;
            raceEvent.EventType = dto.EventType;
            raceEvent.BannerImageUrl = dto.BannerImageUrl;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated.",
                raceEvent.EventId
            });
        }

        // DELETE: api/events/5
        [HttpDelete("{eventId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            var raceEvent = await _context.Events
                .FindAsync(eventId);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.OrganiserId != CurrentUserId)
                return Forbid();

            bool hasCategories = await _context.Categories
                .AnyAsync(c => c.EventId == eventId);

            bool hasEnrolments = await _context.EventEnrolments
                .AnyAsync(e => e.EventId == eventId);

            if (hasCategories || hasEnrolments)
            {
                return Conflict(new
                {
                    message = "Cannot delete an event with categories or enrolments."
                });
            }

            _context.Events.Remove(raceEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
