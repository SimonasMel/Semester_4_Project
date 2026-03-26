using BackEnd.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ↓ FIX 1: Register the repository so the controller can use it
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

app.UseSwagger();
app.UseSwaggerUI();

// ↓ FIX 3: Apply the CORS policy (must be before UseAuthorization)
app.UseCors("AllowBlazor");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
public partial class Program { }