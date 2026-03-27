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
        public int RemainingCars { get; set; } = 0;
        public event Action? OnChange;

        public CarService(HttpClient http)
        {
            _http = http;
        }

        // ── API calls ──────────────────────────────────────────
        public async Task<List<Car>> GetAllCarsAsync()
        {
            try
            {
                var result = await _http.GetFromJsonAsync<List<Car>>("api/cars");
                return result ?? new List<Car>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cars: {ex.Message}");
                throw;
            }
        }

        public async Task<Car?> GetCarByIdAsync(string id)
        {
            try
            {
                return await _http.GetFromJsonAsync<Car>($"api/cars/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting car: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateCarAsync(Car car)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/cars", car);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error creating car. Status: {response.StatusCode}");
                    Console.WriteLine($"Response: {errorContent}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating car: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> UpdateCarAsync(string id, Car car)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/cars/{id}", car);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating car: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteCarAsync(string id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/cars/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error deleting car. Status: {response.StatusCode}");
                    Console.WriteLine($"Response: {errorContent}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting car: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
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

        public void UpdateRemainingCars(int count)
        {
            RemainingCars = count;
            OnChange?.Invoke();
        }
    }
}