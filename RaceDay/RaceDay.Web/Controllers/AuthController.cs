
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using System.Security.Claims;

namespace RaceDay.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiService _authService;

        public AuthController(AuthApiService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (user, apiCookie, error) =
                await _authService.LoginAsync(model);

            if (user == null || apiCookie == null)
            {
                ModelState.AddModelError(
                    "", error ?? "Login failed.");

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.FullName),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            HttpContext.Session.SetString(
                "ApiAuthCookie", apiCookie);

            if (user.Role == "Organiser")
                return RedirectToAction("Index", "Organiser");

            return RedirectToAction("Index", "Participant");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Role != "Organiser" &&
                model.Role != "Participant")
            {
                ModelState.AddModelError(
                    "Role", "Invalid role.");

                return View(model);
            }

            using var response =
                await _authService.RegisterAsync(model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "", "Registration failed. Check your details.");

                return View(model);
            }

            TempData["Success"] =
                "Registration successful. Please log in.";

            return RedirectToAction("Login");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
