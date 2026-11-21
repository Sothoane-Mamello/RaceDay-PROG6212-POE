
using Microsoft.AspNetCore.Authentication.Cookies;
using RaceDay.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// 1. MVC Controllers and Views
// =====================================

builder.Services.AddControllersWithViews();

// =====================================
// 2. Session Management
// =====================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// =====================================
// 3. Connect to RaceDay Part 2 API
// =====================================

builder.Services.AddHttpClient("RaceDayApi", client =>
{
    var apiUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException(
            "API BaseUrl is missing in appsettings.json.");

    client.BaseAddress = new Uri(apiUrl);

    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
            "application/json"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        // Prevent cookies being shared between users
        UseCookies = false,

        AllowAutoRedirect = false
    });

// =====================================
// 4. Register API Services
// =====================================

builder.Services.AddScoped<AuthApiService>();

// =====================================
// 5. Cookie Authentication
// =====================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";

        options.AccessDeniedPath = "/Auth/AccessDenied";

        options.Cookie.Name = "RaceDayWebAuth";

        options.Cookie.HttpOnly = true;

        options.Cookie.SameSite = SameSiteMode.Lax;

        // HTTP allowed temporarily for local testing
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;

        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// =====================================
// 6. Role-Based Authorization
// =====================================

builder.Services.AddAuthorization();

// =====================================
// 7. Build Application
// =====================================

var app = builder.Build();

// =====================================
// 8. Error Handling
// =====================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// =====================================
// 9. Middleware
// =====================================

// TEMPORARILY DISABLED FOR HTTP TESTING
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

// =====================================
// 10. MVC Routing
// =====================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// =====================================
// 11. Run Application
// =====================================

app.Run();
