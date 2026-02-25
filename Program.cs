using Test.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

// --- PROJ-19: Setting the structure ---
string dataDirectory = "Data";
string filePath = Path.Combine(dataDirectory, "cars.json");

// Ensure data file exists
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}

// Data load-up
List<string> cars = LoadFromFile(filePath);

Console.WriteLine("=== Automobilių valdymo sistema ===");
Console.WriteLine();

// Form for adding cars
Console.Write("Įveskite automobilio pavadinimą (pvz., Audi A6): ");
string input = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(input))
{
    cars.Add(input);
    SaveToFile(filePath, cars);
    Console.WriteLine($"\nSėkmingai pridėta: {input}");
}
else
{
    Console.WriteLine("\nKlaida: Pavadinimas negali būti tuščias.");
}

// Show the whole list
Console.WriteLine("\nAktualus automobilių sąrašas:");
foreach (var car in cars)
{
    Console.WriteLine($"- {car}");
}

Console.WriteLine("\nSpauskite bet kurį klavišą, kad uždarytumėte...");
Console.ReadKey();

// --- Helping methods ---

static void SaveToFile(string path, List<string> data)
{
    string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(path, jsonString);
}

static List<string> LoadFromFile(string path)
{
    if (!File.Exists(path)) return new List<string>();
    
    try 
    {
        string jsonString = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<string>>(jsonString) ?? new List<string>();
    }
    catch 
    {
        return new List<string>(); //if file doesn't exist 
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register HttpClient so components can inject System.Net.Http.HttpClient
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp => sp.GetRequiredService<System.Net.Http.IHttpClientFactory>().CreateClient());

// Add this near your other builder.Services lines
builder.Services.AddSingleton<CarService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

