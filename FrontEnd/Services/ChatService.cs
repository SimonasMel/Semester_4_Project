using Microsoft.AspNetCore.SignalR.Client;
using Shared.Models;

namespace FrontEnd.Services
{
    /// <summary>
    /// Manages the SignalR connection to the backend ChatHub.
    /// Handles connecting, sending messages, and receiving real-time updates.
    /// </summary>
    public class ChatService : IAsyncDisposable
    {
        private readonly IConfiguration _config;
        private readonly AuthTokenStore _tokenStore;
        private readonly ILogger<ChatService> _logger;

        private HubConnection? _hubConnection;
        private string? _currentMatchId;

        // Events that components subscribe to
        public event Action<ChatMessage>? OnMessageReceived;
        public event Action<bool>? OnConnectionStateChanged;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public ChatService(IConfiguration config, AuthTokenStore tokenStore, ILogger<ChatService> logger)
        {
            _config = config;
            _tokenStore = tokenStore;
            _logger = logger;
        }

        /// <summary>
        /// Opens a chat for a specific match.
        /// Connects to the hub if not already connected, then joins the match group.
        /// </summary>
        public async Task OpenChatAsync(string matchId)
        {
            // Leave previous match group if switching chats
            if (_currentMatchId != null && _currentMatchId != matchId && IsConnected)
            {
                await _hubConnection!.InvokeAsync("LeaveMatch", _currentMatchId);
            }

            _currentMatchId = matchId;

            if (_hubConnection == null || _hubConnection.State == HubConnectionState.Disconnected)
            {
                await ConnectAsync();
            }

            if (IsConnected)
            {
                await _hubConnection!.InvokeAsync("JoinMatch", matchId);
            }
        }

        /// <summary>
        /// Closes the chat for the current match.
        /// </summary>
        public async Task CloseChatAsync()
        {
            if (_currentMatchId != null && IsConnected)
            {
                await _hubConnection!.InvokeAsync("LeaveMatch", _currentMatchId);
                _currentMatchId = null;
            }
        }

        /// <summary>
        /// Sends a message in the current match.
        /// </summary>
        public async Task SendMessageAsync(string matchId, string content)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to chat.");

            await _hubConnection!.InvokeAsync("SendMessage", matchId, content);
        }

        /// <summary>
        /// Marks messages in a match as read.
        /// </summary>
        public async Task MarkAsReadAsync(string matchId)
        {
            if (IsConnected)
                await _hubConnection!.InvokeAsync("MarkAsRead", matchId);
        }

        private async Task ConnectAsync()
        {
            var backendUrl = _config["Backend:ApiUrl"] ?? "https://localhost:7065";
            var token = _tokenStore.Token;

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Cannot connect to ChatHub - no auth token available");
                return;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{backendUrl}/hubs/chat", options =>
                {
                    // Pass JWT token as query param (required for SignalR WebSocket auth)
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                })
                .WithAutomaticReconnect()
                .Build();

            // Wire up incoming message handler
            _hubConnection.On<ChatMessage>("ReceiveMessage", message =>
            {
                OnMessageReceived?.Invoke(message);
            });

            _hubConnection.Reconnected += connectionId =>
            {
                _logger.LogInformation("ChatHub reconnected: {ConnectionId}", connectionId);
                OnConnectionStateChanged?.Invoke(true);
                // Re-join the match group after reconnect
                if (_currentMatchId != null)
                    _ = _hubConnection.InvokeAsync("JoinMatch", _currentMatchId);
                return Task.CompletedTask;
            };

            _hubConnection.Closed += error =>
            {
                _logger.LogWarning("ChatHub connection closed: {Error}", error?.Message);
                OnConnectionStateChanged?.Invoke(false);
                return Task.CompletedTask;
            };

            try
            {
                await _hubConnection.StartAsync();
                OnConnectionStateChanged?.Invoke(true);
                _logger.LogInformation("Connected to ChatHub");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to ChatHub");
                OnConnectionStateChanged?.Invoke(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
