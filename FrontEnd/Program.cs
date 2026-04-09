using FrontEnd.Components;
using FrontEnd.Services;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(
    new FileErrorLoggerProvider(Path.Combine(builder.Environment.ContentRootPath, "..", "Logs", "errors.log")));

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register HttpClient for CarService
builder.Services.AddScoped(sp =>
{
    var handler = new HttpClientHandler();
    // Allow self-signed certificates in development
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    }

    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri("https://localhost:7065")
    };

    return httpClient;
});

// App services
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<ThemeService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();