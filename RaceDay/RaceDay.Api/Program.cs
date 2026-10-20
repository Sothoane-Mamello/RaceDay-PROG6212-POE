
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SQL Server database
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "RaceDayConnection"
        )
    )
);

// Cookie authentication
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .AddCookie(options =>
    {
        options.Cookie.Name = "RaceDayAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;

        // Allows local HTTP testing.
        // Production must use HTTPS and secure cookies.
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;

        options.ExpireTimeSpan = TimeSpan.FromHours(2);

        // Return API status codes instead of login redirects
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

// Role-based authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Temporarily disabled for local HTTP testing
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
