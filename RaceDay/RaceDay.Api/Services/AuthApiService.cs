
using System.Net;
using System.Net.Http.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services
{
    public class AuthApiService
    {
        private readonly IHttpClientFactory _factory;

        public AuthApiService(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task<HttpResponseMessage> RegisterAsync(
            RegisterViewModel model)
        {
            var client = _factory.CreateClient("RaceDayApi");

            return await client.PostAsJsonAsync(
                "api/auth/register", model);
        }

        public async Task<(AuthResponse? User,
            string? ApiCookie, string? Error)> LoginAsync(
            LoginViewModel model)
        {
            var client = _factory.CreateClient("RaceDayApi");

            using var request = new HttpRequestMessage(
                HttpMethod.Post, "api/auth/login");

            request.Content = JsonContent.Create(model);

            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return (null, null,
                    "Invalid email or password.");
            }

            var user = await response.Content
                .ReadFromJsonAsync<AuthResponse>();

            string? cookie = null;

            if (response.Headers.TryGetValues(
                "Set-Cookie", out var cookies))
            {
                var authCookie = cookies.FirstOrDefault(
                    c => c.StartsWith(
                        "RaceDayAuth=",
                        StringComparison.OrdinalIgnoreCase));

                cookie = authCookie?
                    .Split(';', 2)[0];
            }

            if (user == null || string.IsNullOrEmpty(cookie))
            {
                return (null, null,
                    "API authentication cookie was not returned.");
            }

            return (user, cookie, null);
        }
    }
}
