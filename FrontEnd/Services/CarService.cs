// Services/CarService.cs
using System.Net.Http.Json;
using Shared.Models;

namespace FrontEnd.Services
{
    public class CarService
    {
        private readonly HttpClient _http;
        private readonly ILogger<CarService> _logger;
        private readonly HashSet<string> SwipedCarIds = new();

        // Matched/liked cars stored in memory
        public List<Car> MatchedCars { get; private set; } = new();
        public int RemainingCars { get; set; } = 0;
        public event Action? OnChange;

        public CarService(HttpClient http, ILogger<CarService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public bool IsSwiped(string carId) => SwipedCarIds.Contains(carId);

        private bool MarkSwiped(Car car) => SwipedCarIds.Add(car.Id);

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
                _logger.LogError(ex, "Error getting cars");
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
                _logger.LogError(ex, "Error getting car {CarId}", id);
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
                    _logger.LogError("Error creating car. Status: {StatusCode}. Response: {Response}", response.StatusCode, errorContent);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating car {Brand} {Model}", car.Brand, car.Model);
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
                _logger.LogError(ex, "Error updating car {CarId}", id);
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
                    _logger.LogError("Error deleting car. Status: {StatusCode}. Response: {Response}", response.StatusCode, errorContent);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting car {CarId}", id);
                throw;
            }
        }

        // ── Match/garage tracking ──────────────────────────────
        public void AddMatch(Car car)
        {
            var changed = MarkSwiped(car);

            if (!MatchedCars.Any(c => c.Id == car.Id))
            {
                MatchedCars.Add(car);
                changed = true;
            }

            if (changed)
            {
                OnChange?.Invoke();
            }
        }

        public void AddSwipe(Car car)
        {
            if (MarkSwiped(car))
            {
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