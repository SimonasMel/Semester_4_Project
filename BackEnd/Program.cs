using BackEnd.Repositories;
using BackEnd.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(
    new FileErrorLoggerProvider(Path.Combine(builder.Environment.ContentRootPath, "..", "Logs", "errors.log")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ↓ FIX 0: Add Entity Framework Core with SQLite
builder.Services.AddDbContext<CarDbContext>(options =>
    options.UseSqlite("Data Source=cars.db"));

// ↓ FIX 1: Register the repository as Scoped (not Singleton) to work with Scoped DbContext
builder.Services.AddScoped<ICarRepository, CarRepository>();

// ↓ FIX 2: Allow Blazor frontend to call this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
        policy.WithOrigins("https://localhost:7140")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// ↓ NEW: Automatically create and migrate the database on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<CarDbContext>();
    try
    {
        // Apply any pending migrations
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying migrations");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

// ↓ FIX 3: Apply the CORS policy (must be before UseAuthorization)
app.UseCors("AllowBlazor");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
public partial class Program { }