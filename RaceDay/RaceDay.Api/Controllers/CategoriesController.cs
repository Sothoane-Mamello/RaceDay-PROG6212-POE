
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
    [Route("api/categories")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public CategoriesController(RaceDayDbContext db)
        {
            _db = db;
        }

        private int UserId =>
            int.Parse(User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        // GET: api/categories/event/2
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            bool exists = await _db.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!exists)
                return NotFound(new { message = "Event not found." });

            var categories = await _db.Categories
                .AsNoTracking()
                .Where(c => c.EventId == eventId)
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.Name,
                    c.CategoryType,
                    c.MinAge,
                    c.MaxAge,
                    c.DistanceKm,
                    c.Description
                })
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/categories/event/2
        [HttpPost("event/{eventId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create(
            int eventId, CategoryDto dto)
        {
            var raceEvent = await _db.Events
                .FindAsync(eventId);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.OrganiserId != UserId)
                return Forbid();

            if (!ValidCategory(dto))
                return BadRequest(new
                {
                    message = "Invalid age or distance category."
                });

            bool duplicate = await _db.Categories
                .AnyAsync(c =>
                    c.EventId == eventId && c.Name == dto.Name);

            if (duplicate)
                return Conflict(new
                {
                    message = "Category name already exists."
                });

            var category = new Category
            {
                EventId = eventId,
                Name = dto.Name,
                CategoryType = dto.CategoryType,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge,
                DistanceKm = dto.DistanceKm,
                Description = dto.Description
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Category created.",
                category.CategoryId,
                category.EventId
            });
        }

        // PUT: api/categories/1
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(
            int id, CategoryDto dto)
        {
            var category = await _db.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            if (category.Event.OrganiserId != UserId)
                return Forbid();

            if (!ValidCategory(dto))
                return BadRequest(new
                {
                    message = "Invalid category."
                });

            bool duplicate = await _db.Categories
                .AnyAsync(c =>
                    c.EventId == category.EventId &&
                    c.CategoryId != id &&
                    c.Name == dto.Name);

            if (duplicate)
                return Conflict(new
                {
                    message = "Category name already exists."
                });

            category.Name = dto.Name;
            category.CategoryType = dto.CategoryType;
            category.MinAge = dto.MinAge;
            category.MaxAge = dto.MaxAge;
            category.DistanceKm = dto.DistanceKm;
            category.Description = dto.Description;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Category updated." });
        }

        // DELETE: api/categories/1
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            if (category.Event.OrganiserId != UserId)
                return Forbid();

            bool enrolled = await _db.EventEnrolments
                .AnyAsync(e => e.CategoryId == id);

            if (enrolled)
                return Conflict(new
                {
                    message = "Category has enrolments."
                });

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private static bool ValidCategory(CategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return false;

            if (dto.CategoryType == "Age")
            {
                if (dto.DistanceKm != null)
                    return false;

                if (dto.MinAge == null && dto.MaxAge == null)
                    return false;

                if (dto.MinAge < 0 || dto.MaxAge < 0)
                    return false;

                return dto.MinAge == null ||
                    dto.MaxAge == null ||
                    dto.MaxAge >= dto.MinAge;
            }

            if (dto.CategoryType == "Distance")
            {
                return dto.DistanceKm > 0 &&
                    dto.MinAge == null &&
                    dto.MaxAge == null;
            }

            return false;
        }
    }
}
