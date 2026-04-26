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
            if (_tokenStore.IsAuthenticated && !string.IsNullOrWhiteSpace(_tokenStore.Token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenStore.Token);
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = null;
            }
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
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized when fetching cars. User may not be authenticated yet.");
                return new List<Car>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cars");
                return new List<Car>();
            }
        }

        public async Task<List<Car>> GetUserCarsAsync()
        {
            try
            {
                await EnsureUserDataLoadedAsync();
                ApplyAuthHeader();
                var result = await _http.GetFromJsonAsync<List<Car>>("api/cars/my");
                return result ?? new List<Car>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized when fetching user cars.");
                return new List<Car>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user cars");
                return new List<Car>();
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
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized when fetching car {CarId}.", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting car {CarId}", id);
                return null;
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

        public class UploadResult
        {
            public List<string> Urls { get; set; } = new();
        }

        public async Task<List<string>> UploadCarImagesAsync(MultipartFormDataContent content)
        {
            try
            {
                ApplyAuthHeader(); // Optional depending on backend AllowAnonymous config
                var response = await _http.PostAsync("api/cars/upload", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UploadResult>();
                    return result?.Urls ?? new List<string>();
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error uploading images. Status: {StatusCode}. Response: {Response}", response.StatusCode, errorContent);
                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading images");
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
        //Preferences
        private UserPreferences? _preferences;

        public async Task<UserPreferences?> GetPreferencesAsync()
        {
            ApplyAuthHeader(); 
            var response = await _http.GetAsync("api/preferences");
            if (response.IsSuccessStatusCode)
                _preferences = await response.Content.ReadFromJsonAsync<UserPreferences>();
            return _preferences;
        }

        public async Task SavePreferencesAsync(UserPreferences prefs)
        {
            ApplyAuthHeader(); 
            await _http.PostAsJsonAsync("api/preferences", prefs);
            _preferences = prefs;
        }

        public int ScoreCar(Car car)
        {
            if (_preferences == null) return 0;
            int score = 0;

            if (_preferences.UseBrand && !string.IsNullOrEmpty(_preferences.PreferredBrand) &&
                car.Brand.Equals(_preferences.PreferredBrand, StringComparison.OrdinalIgnoreCase)) score += 3;

            if (_preferences.UseFuelType && _preferences.FuelType.HasValue &&
                car.FuelType == _preferences.FuelType) score += 2;

            if (_preferences.UseTransmission && _preferences.Transmission.HasValue &&
                car.Transmission == _preferences.Transmission) score += 2;

            if (_preferences.UseBodyType && _preferences.BodyType.HasValue &&
                car.BodyType == _preferences.BodyType) score += 2;

            if (_preferences.UseYear && _preferences.PreferredYear.HasValue)
            {
                var yearDiff = Math.Abs(car.ProductionYear - _preferences.PreferredYear.Value);
                if (yearDiff == 0) score += 3;
                else if (yearDiff <= 2) score += 2;
                else if (yearDiff <= 5) score += 1;
            }

            if (_preferences.UsePrice && _preferences.PreferredPrice.HasValue && _preferences.PreferredPrice > 0)
            {
                var priceDiff = Math.Abs(car.Price - _preferences.PreferredPrice.Value);
                var pricePercent = priceDiff / _preferences.PreferredPrice.Value * 100;
                if (pricePercent <= 5) score += 3;
                else if (pricePercent <= 15) score += 2;
                else if (pricePercent <= 30) score += 1;
            }

            if (_preferences.UseMileage && _preferences.MileageKm.HasValue)
            {
                var mileageDiff = Math.Abs(car.MileageKm - _preferences.MileageKm.Value);
                var mileagePercent = mileageDiff / (double)Math.Max(_preferences.MileageKm.Value, 1) * 100;
                if (mileagePercent <= 5) score += 3;
                else if (mileagePercent <= 20) score += 2;
                else if (mileagePercent <= 40) score += 1;
            }

            if (_preferences.UseEnginePower && _preferences.EnginePowerKW.HasValue)
            {
                var powerDiff = Math.Abs(car.EnginePowerKW - _preferences.EnginePowerKW.Value);
                var powerPercent = powerDiff / (double)Math.Max(_preferences.EnginePowerKW.Value, 1) * 100;
                if (powerPercent <= 5) score += 3;
                else if (powerPercent <= 20) score += 2;
                else if (powerPercent <= 40) score += 1;
            }

            return score;
        }

        public List<Car> GetSortedCars(List<Car> cars)
        {
            return cars.OrderByDescending(ScoreCar).ToList();
        }

        // ── Mutual Matches ────────────────────────────────────
        private List<MutualMatch> _mutualMatches = new();

        private static string GetMutualMatchesStorageKey(string userKey) => $"mutual-matches:{userKey}";

        /// <summary>
        /// Called when current user likes another user's car.
        /// Checks if the other user has also liked any of the current user's cars.
        /// </summary>
        public async Task<MutualMatch?> LikeCarAsync(string carId, string carOwnerId)
        {
            try
            {
                ApplyAuthHeader();
                var response = await _http.PostAsJsonAsync($"api/cars/{carId}/like", new { ownerId = carOwnerId });

                if (response.IsSuccessStatusCode)
                {
                    var mutualMatch = await response.Content.ReadFromJsonAsync<MutualMatch>();
                    if (mutualMatch != null)
                    {
                        _mutualMatches.Add(mutualMatch);
                        await PersistMutualMatchesAsync();
                        OnChange?.Invoke();
                        return mutualMatch;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking car {CarId}", carId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all mutual matches for the current user.
        /// </summary>
        public async Task<List<MutualMatch>> GetMutualMatchesAsync()
        {
            try
            {
                await EnsureUserDataLoadedAsync();
                ApplyAuthHeader();
                try
                {
                    var result = await _http.GetFromJsonAsync<List<MutualMatch>>("api/matches/mutual");
                    if (result != null)
                    {
                        _mutualMatches = result;
                        await PersistMutualMatchesAsync();
                    }
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Mutual matches endpoint not found. This feature may not be implemented on the backend yet.");
                    // Return cached mutual matches if endpoint doesn't exist
                }
                return _mutualMatches;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mutual matches");
                return _mutualMatches;
            }
        }

        /// <summary>
        /// Removes a mutual match (user can "unmatch" from someone).
        /// </summary>
        public async Task<bool> RemoveMutualMatchAsync(string matchId)
        {
            try
            {
                ApplyAuthHeader();
                var response = await _http.DeleteAsync($"api/matches/mutual/{matchId}");

                if (response.IsSuccessStatusCode)
                {
                    _mutualMatches.RemoveAll(m => m.Id == matchId);
                    await PersistMutualMatchesAsync();
                    OnChange?.Invoke();
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing mutual match {MatchId}", matchId);
                throw;
            }
        }

        /// <summary>
        /// Loads mutual matches from browser storage and API.
        /// </summary>
        private async Task LoadMutualMatchesAsync()
        {
            try
            {
                var userKey = GetUserKey();
                var matchesRaw = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", GetMutualMatchesStorageKey(userKey));
                if (!string.IsNullOrWhiteSpace(matchesRaw))
                {
                    var matches = JsonSerializer.Deserialize<List<MutualMatch>>(matchesRaw) ?? new List<MutualMatch>();
                    _mutualMatches = matches.Where(m => m.IsActive).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading mutual matches from browser storage");
            }
        }

        private async Task PersistMutualMatchesAsync()
        {
            try
            {
                var userKey = _loadedUserKey ?? GetUserKey();
                var payload = JsonSerializer.Serialize(_mutualMatches);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", GetMutualMatchesStorageKey(userKey), payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving mutual matches to browser storage");
            }
        }

        /// <summary>
        /// Gets the list of mutual matches from memory.
        /// </summary>
        public List<MutualMatch> GetMutualMatches() => _mutualMatches;
    }
}
