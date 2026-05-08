using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System.Security.Claims;

namespace BackEnd.Hubs
{
    /// <summary>
    /// SignalR hub for real-time chat between matched users.
    /// Each match has its own group — only the two matched users can join it.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly CarDbContext _db;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(CarDbContext db, ILogger<ChatHub> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Called when a user opens a chat for a specific match.
        /// Validates the user belongs to the match before joining the group.
        /// </summary>
        public async Task JoinMatch(string matchId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("Not authenticated.");
            }

            // Verify this user is part of the match
            var match = await _db.MutualMatches
                .FirstOrDefaultAsync(m => m.Id == matchId && m.IsActive &&
                    (m.CurrentUserId == userId || m.MatchedUserId == userId));

            if (match == null)
            {
                throw new HubException("Match not found or you are not part of this match.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
            _logger.LogInformation("User {UserId} joined chat group {MatchId}", userId, matchId);
        }

        /// <summary>
        /// Called when a user leaves a chat (closes the panel).
        /// </summary>
        public async Task LeaveMatch(string matchId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, matchId);
        }

        /// <summary>
        /// Sends a message to both users in a match.
        /// Saves to DB and broadcasts to the match group.
        /// </summary>
        public async Task SendMessage(string matchId, string content)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new HubException("Not authenticated.");

            if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
                throw new HubException("Invalid message content.");

            // Verify user is part of this match
            var match = await _db.MutualMatches
                .FirstOrDefaultAsync(m => m.Id == matchId && m.IsActive &&
                    (m.CurrentUserId == userId || m.MatchedUserId == userId));

            if (match == null)
                throw new HubException("Match not found or you are not part of this match.");

            // Save to database
            var message = new ChatMessage
            {
                MatchId = matchId,
                SenderId = userId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _db.ChatMessages.Add(message);
            await _db.SaveChangesAsync();

            // Broadcast to everyone in the match group (both users)
            await Clients.Group(matchId).SendAsync("ReceiveMessage", message);

            _logger.LogInformation("Message sent in match {MatchId} by user {UserId}", matchId, userId);
        }

        /// <summary>
        /// Marks all messages in a match as read for the current user.
        /// </summary>
        public async Task MarkAsRead(string matchId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return;

            var unread = await _db.ChatMessages
                .Where(m => m.MatchId == matchId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            foreach (var msg in unread)
                msg.IsRead = true;

            await _db.SaveChangesAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
