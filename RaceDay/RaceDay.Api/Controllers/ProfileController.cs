
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public ProfileController(RaceDayDbContext db)
        {
            _db = db;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        // GET: api/profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _db.Users
                .Include(u => u.Organiser)
                .Include(u => u.Participant)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user == null || !user.IsActive)
                return NotFound();

            return Ok(new
            {
                user.UserId,
                user.FullName,
                user.Email,
                user.Role,
                user.ProfileImageUrl,
                PhoneNumber = user.Role == "Organiser"
                    ? user.Organiser?.PhoneNumber
                    : user.Participant?.PhoneNumber,
                OrganisationName =
                    user.Organiser?.OrganisationName,
                DateOfBirth = user.Participant?.DateOfBirth,
                EmergencyContact =
                    user.Participant?.EmergencyContact
            });
        }

        // PUT: api/profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            UpdateProfileDto dto)
        {
            var user = await _db.Users
                .Include(u => u.Organiser)
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user == null || !user.IsActive)
                return NotFound();

            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest(new
                {
                    message = "Full name is required."
                });

            user.FullName = dto.FullName.Trim();
            user.ProfileImageUrl = dto.ProfileImageUrl;

            if (user.Role == "Organiser")
            {
                if (user.Organiser == null)
                    return Conflict(new
                    {
                        message = "Organiser profile not found."
                    });

                user.Organiser.PhoneNumber = dto.PhoneNumber;
                user.Organiser.OrganisationName =
                    dto.OrganisationName;
            }
            else if (user.Role == "Participant")
            {
                if (user.Participant == null)
                    return Conflict(new
                    {
                        message = "Participant profile not found."
                    });

                if (dto.DateOfBirth > DateTime.UtcNow)
                    return BadRequest(new
                    {
                        message = "Invalid date of birth."
                    });

                user.Participant.PhoneNumber = dto.PhoneNumber;
                user.Participant.DateOfBirth = dto.DateOfBirth;
                user.Participant.EmergencyContact =
                    dto.EmergencyContact;
            }
            else
            {
                return Forbid();
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                user.UserId,
                user.FullName,
                user.Role
            });
        }
    }
}
