using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using FrontEnd.Contracts.Auth;
using Microsoft.JSInterop;

namespace FrontEnd.Services
{
    public class AuthService
    {
        private const string TokenStorageKey = "auth.token";
        private const string EmailStorageKey = "auth.email";

        private readonly HttpClient _http;
        private readonly AuthTokenStore _tokenStore;
        private readonly ApiAuthStateProvider _authStateProvider;
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            HttpClient http,
            AuthTokenStore tokenStore,
            ApiAuthStateProvider authStateProvider,
            IJSRuntime jsRuntime,
            ILogger<AuthService> logger)
        {
            _http = http;
            _tokenStore = tokenStore;
            _authStateProvider = authStateProvider;
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", TokenStorageKey);
                var email = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", EmailStorageKey);

                if (string.IsNullOrWhiteSpace(token) || IsTokenExpired(token))
                {
                    await LogoutAsync();
                    return;
                }

                _tokenStore.SetToken(token, email);
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _authStateProvider.NotifyUserAuthentication(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing auth session");
                await LogoutAsync();
            }
        }

        public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/register", request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return (false, error);
                }

                var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (auth is null || string.IsNullOrWhiteSpace(auth.Token))
                {
                    return (false, "Invalid register response from server.");
                }

                await SetAuthenticatedUserAsync(auth);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during register");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return (false, error);
                }

                var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (auth is null || string.IsNullOrWhiteSpace(auth.Token))
                {
                    return (false, "Invalid login response from server.");
                }

                await SetAuthenticatedUserAsync(auth);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return (false, ex.Message);
            }
        }

        public async Task LogoutAsync()
        {
            _tokenStore.Clear();
            _http.DefaultRequestHeaders.Authorization = null;
            _authStateProvider.NotifyUserLogout();

            try
            {
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", TokenStorageKey);
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", EmailStorageKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing auth session");
            }
        }

        public async Task<(bool Success, string? Email)> GetCurrentUserAsync()
        {
            try
            {
                if (!_tokenStore.IsAuthenticated)
                {
                    return (false, null);
                }

                var response = await _http.GetAsync("api/auth/me");
                if (!response.IsSuccessStatusCode)
                {
                    return (false, null);
                }

                var payload = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
                return payload is null ? (false, null) : (true, payload.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return (false, null);
            }
        }

        private async Task SetAuthenticatedUserAsync(AuthResponse auth)
        {
            _tokenStore.SetToken(auth.Token, auth.Email);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
            _authStateProvider.NotifyUserAuthentication(auth.Token);

            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", TokenStorageKey, auth.Token);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", EmailStorageKey, auth.Email ?? string.Empty);
        }

        private static bool IsTokenExpired(string token)
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                return jwt.ValidTo <= DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        private class CurrentUserResponse
        {
            public string Email { get; set; } = string.Empty;
        }
    }
}
