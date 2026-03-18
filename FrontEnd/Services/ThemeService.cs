using System;
using Microsoft.JSInterop;

namespace FrontEnd.Services
{
    public class ThemeService
    {
        private readonly IJSRuntime _js;
        private bool _isInitialized;
        public bool IsDark { get; private set; } = true;

        public event Action? OnChange;

        public ThemeService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                IsDark = await _js.InvokeAsync<bool>("themeManager.getTheme");
                _isInitialized = true;
                OnChange?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Theme initialization error: {ex.Message}");
            }
        }

        public async Task ToggleAsync()
        {
            await SetDarkAsync(!IsDark);
        }

        public async Task SetDarkAsync(bool isDark)
        {
            IsDark = isDark;
            _isInitialized = true; // Mark as initialized if user manually toggles
            try
            {
                await _js.InvokeVoidAsync("themeManager.setTheme", isDark);
                OnChange?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Theme toggle error: {ex.Message}");
            }
        }
    }
}
