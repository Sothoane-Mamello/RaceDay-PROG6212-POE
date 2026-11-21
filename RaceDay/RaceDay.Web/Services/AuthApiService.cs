
using System.Net.Http.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services
{
    public class AuthApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthApiService(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HttpResponseMessage> RegisterAsync(
            RegisterViewModel model)
        {
            var client = _httpClientFactory
                .CreateClient("RaceDayApi");

            return await client.PostAsJsonAsync(
                "api/auth/register",
                new
                {
                    model.FullName,
                    model.Email,
                    model.Password,
                    model.Role
                });
        }

        public async Task<(
            AuthResponse? User,
            string? Cookie,
            string? Error)> LoginAsync(
                LoginViewModel model)
        {
            var client = _httpClientFactory
                .CreateClient("RaceDayApi");

            using var response = await client.PostAsJsonAsync(
                "api/auth/login", model);

            if (!response.IsSuccessStatusCode)
            {
                return (null, null,
                    "Invalid email or password.");
            }

            var user = await response.Content
                .ReadFromJsonAsync<AuthResponse>();

            string? cookie = null;

            if (response.Headers.TryGetValues(
                "Set-Cookie", out var values))
            {
                cookie = values
                    .FirstOrDefault(x => x.StartsWith(
                        "RaceDayAuth=",
                        StringComparison.OrdinalIgnoreCase))
                    ?.Split(';')[0];
            }

            if (user == null || cookie == null)
            {
                return (null, null,
                    "Could not establish an API session.");
            }

            return (user, cookie, null);
        }
    }
}
