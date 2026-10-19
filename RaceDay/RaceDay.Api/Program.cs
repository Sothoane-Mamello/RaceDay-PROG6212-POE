
var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger during development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection temporarily disabled for local testing
// app.UseHttpsRedirection();

// Enable authorization middleware
app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Start the application
app.Run();
