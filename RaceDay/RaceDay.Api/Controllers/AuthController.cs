
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            string email = dto.Email.Trim().ToLowerInvariant();
            string role = dto.Role.Trim();

            if (role != "Organiser" && role != "Participant")
            {
                return BadRequest(new
                {
                    message = "Role must be Organiser or Participant."
                });
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return Conflict(new
                {
                    message = "Email already registered."
                });
            }

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                Role = role,
                IsActive = true
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(user, dto.Password);

            if (role == "Organiser")
            {
                user.Organiser = new Organiser();
            }
            else
            {
                user.Participant = new Participant();
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Registration successful.",
                userId = user.UserId,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            string email = dto.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var verification =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    dto.Password
                );

            if (verification == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            if (verification ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash =
                    _passwordHasher.HashPassword(user, dto.Password);

                await _context.SaveChangesAsync();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(ClaimTypes.Name, user.FullName),

                new Claim(ClaimTypes.Email, user.Email),

                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                }
            );

            return Ok(new
            {
                message = "Login successful.",
                userId = user.UserId,
                fullName = user.FullName,
                role = user.Role
            });
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return NoContent();
        }

        // GET: api/auth/me
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier),

                fullName = User.Identity?.Name,

                role = User.FindFirstValue(ClaimTypes.Role),

                email = User.FindFirstValue(ClaimTypes.Email)
            });
        }
    }
}
