using FrontEnd.Components;
using FrontEnd.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(
    new FileErrorLoggerProvider(Path.Combine(builder.Environment.ContentRootPath, "..", "Logs", "errors.log")));

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthTokenStore>();
builder.Services.AddScoped<ApiAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ApiAuthStateProvider>());
builder.Services.AddScoped<AuthService>();

// Register HttpClient for CarService
builder.Services.AddScoped(sp =>
{
    var backendUrl = builder.Configuration["Backend:ApiUrl"] ?? "https://localhost:7065";
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(backendUrl)
    };

    return httpClient;
});

// App services
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<ThemeService>();

var app = builder.Build();

// ↓ Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline'");
    }
    await next();
});

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    // In development, still use exception handler but with more verbose output
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();