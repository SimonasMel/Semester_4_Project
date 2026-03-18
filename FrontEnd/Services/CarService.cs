// Services/CarService.cs
using System.Net.Http.Json;
using Shared.Models;

namespace FrontEnd.Services
{
    public class CarService
    {
        private readonly HttpClient _http;

        // Matched/liked cars stored in memory
        public List<Car> MatchedCars { get; private set; } = new();
        public event Action? OnChange;

        public CarService(HttpClient http)
        {
            _http = http;
        }

        // ── API calls ──────────────────────────────────────────
        public async Task<List<Car>> GetAllCarsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<Car>>("api/cars");
            return result ?? new List<Car>();
        }

        public async Task<Car?> GetCarByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<Car>($"api/cars/{id}");
        }

        public async Task<bool> CreateCarAsync(Car car)
        {
            var response = await _http.PostAsJsonAsync("api/cars", car);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCarAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/cars/{id}");
            return response.IsSuccessStatusCode;
        }

        // ── Match/garage tracking ──────────────────────────────
        public void AddMatch(Car car)
        {
            if (!MatchedCars.Any(c => c.Id == car.Id))
            {
                MatchedCars.Add(car);
                OnChange?.Invoke();
            }
        }

        public void RemoveMatch(string id)
        {
            MatchedCars.RemoveAll(c => c.Id == id);
            OnChange?.Invoke();
        }
    }
}