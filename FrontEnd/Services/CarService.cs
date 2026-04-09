// Services/CarService.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Shared.Models;

namespace FrontEnd.Services
{
    public class CarService
    {
        private readonly HttpClient _http;
        private readonly AuthTokenStore _tokenStore;
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<CarService> _logger;
        private readonly HashSet<string> SwipedCarIds = new();
        private string? _loadedUserKey;

        public List<Car> MatchedCars { get; private set; } = new();
        public int RemainingCars { get; set; } = 0;
        public event Action? OnChange;

        public CarService(HttpClient http, AuthTokenStore tokenStore, IJSRuntime jsRuntime, ILogger<CarService> logger)
        {
            _http = http;
            _tokenStore = tokenStore;
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public bool IsSwiped(string carId) => SwipedCarIds.Contains(carId);

        private bool MarkSwiped(Car car) => SwipedCarIds.Add(car.Id);

        private void ApplyAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization = _tokenStore.IsAuthenticated && !string.IsNullOrWhiteSpace(_tokenStore.Token)
                ? new AuthenticationHeaderValue("Bearer", _tokenStore.Token)
                : null;
        }

        private string GetUserKey() =>
            string.IsNullOrWhiteSpace(_tokenStore.Email) ? "guest" : _tokenStore.Email.Trim().ToLowerInvariant();

        private static string GetSwipeStorageKey(string userKey) => $"swiped-cars:{userKey}";

        private static string GetMatchesStorageKey(string userKey) => $"matched-cars:{userKey}";

        private async Task EnsureUserDataLoadedAsync()
        {
            var userKey = GetUserKey();
            if (string.Equals(_loadedUserKey, userKey, StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                SwipedCarIds.Clear();
                MatchedCars.Clear();

                var swipeRaw = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", GetSwipeStorageKey(userKey));
                if (!string.IsNullOrWhiteSpace(swipeRaw))
                {
                    var ids = JsonSerializer.Deserialize<List<string>>(swipeRaw) ?? new List<string>();
                    foreach (var id in ids.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        SwipedCarIds.Add(id);
                    }
                }

                var matchesRaw = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", GetMatchesStorageKey(userKey));
                if (!string.IsNullOrWhiteSpace(matchesRaw))
                {
                    var matches = JsonSerializer.Deserialize<List<Car>>(matchesRaw) ?? new List<Car>();
                    MatchedCars = matches
                        .Where(c => !string.IsNullOrWhiteSpace(c.Id))
                        .GroupBy(c => c.Id)
                        .Select(g => g.First())
                        .ToList();

                    foreach (var car in MatchedCars)
                    {
                        SwipedCarIds.Add(car.Id);
                    }
                }

                _loadedUserKey = userKey;
                OnChange?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user car data from browser storage");
            }
        }

        private async Task PersistSwipesAsync()
        {
            try
            {
                var userKey = _loadedUserKey ?? GetUserKey();
                var payload = JsonSerializer.Serialize(SwipedCarIds.ToList());
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", GetSwipeStorageKey(userKey), payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving swiped cars to browser storage");
            }
        }

        private async Task PersistMatchesAsync()
        {
            try
            {
                var userKey = _loadedUserKey ?? GetUserKey();
                var payload = JsonSerializer.Serialize(MatchedCars);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", GetMatchesStorageKey(userKey), payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving matched cars to browser storage");
            }
        }

        // ── API calls ──────────────────────────────────────────
        public async Task<List<Car>> GetAllCarsAsync()
        {
            try
            {
                await EnsureUserDataLoadedAsync();
                ApplyAuthHeader();
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
                await EnsureUserDataLoadedAsync();
                ApplyAuthHeader();
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
                await EnsureUserDataLoadedAsync();
                ApplyAuthHeader();
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
                await EnsureUserDataLoadedAsync();
                ApplyAuthHeader();
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
                await EnsureUserDataLoadedAsync();
                ApplyAuthHeader();
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
                _ = PersistSwipesAsync();
                _ = PersistMatchesAsync();
                OnChange?.Invoke();
            }
        }

        public void AddSwipe(Car car)
        {
            if (MarkSwiped(car))
            {
                _ = PersistSwipesAsync();
                OnChange?.Invoke();
            }
        }

        public void RemoveMatch(string id)
        {
            MatchedCars.RemoveAll(c => c.Id == id);
            _ = PersistMatchesAsync();
            OnChange?.Invoke();
        }

        public void UpdateRemainingCars(int count)
        {
            RemainingCars = count;
            OnChange?.Invoke();
        }
    }
}