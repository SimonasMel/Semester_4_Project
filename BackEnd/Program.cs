using BackEnd.Repositories;
using BackEnd.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
    var dbContext = scope.ServiceProvider.GetRequiredService<CarDbContext>();
    try
    {
        // Apply any pending migrations
        dbContext.Database.Migrate();
        Console.WriteLine("✓ Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error applying migrations: {ex.Message}");
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