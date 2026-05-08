using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System.Security.Claims;

namespace BackEnd.Controllers
{
    /// <summary>
    /// REST API for loading chat message history.
    /// Real-time messaging goes through ChatHub (SignalR).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly CarDbContext _db;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(CarDbContext db, ILogger<MessagesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Get message history for a match.
        /// Only returns messages if the current user is part of the match.
        /// </summary>
        [HttpGet("{matchId}")]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessages(
            string matchId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Verify user is part of this match
                var match = await _db.MutualMatches
                    .FirstOrDefaultAsync(m => m.Id == matchId && m.IsActive &&
                        (m.CurrentUserId == userId || m.MatchedUserId == userId));

                if (match == null)
                    return NotFound(new { error = "Match not found or you are not part of this match." });

                var messages = await _db.ChatMessages
                    .Where(m => m.MatchId == matchId)
                    .OrderByDescending(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .OrderBy(m => m.SentAt) // Re-order ascending for display
                    .ToListAsync();

                // Mark messages from the other user as read
                var unread = messages
                    .Where(m => m.SenderId != userId && !m.IsRead)
                    .ToList();

                foreach (var msg in unread)
                    msg.IsRead = true;

                if (unread.Any())
                    await _db.SaveChangesAsync();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading messages for match {MatchId}", matchId);
                return StatusCode(500, new { error = "Failed to load messages." });
            }
        }

        /// <summary>
        /// Get unread message count for all matches of the current user.
        /// Used to show notification badges on the Matches page.
        /// </summary>
        [HttpGet("unread-counts")]
        public async Task<ActionResult<Dictionary<string, int>>> GetUnreadCounts()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Get all active match IDs for this user
                var matchIds = await _db.MutualMatches
                    .Where(m => m.IsActive && (m.CurrentUserId == userId || m.MatchedUserId == userId))
                    .Select(m => m.Id)
                    .ToListAsync();

                // Count unread messages per match (messages not sent by current user)
                var unreadCounts = await _db.ChatMessages
                    .Where(m => matchIds.Contains(m.MatchId) && m.SenderId != userId && !m.IsRead)
                    .GroupBy(m => m.MatchId)
                    .Select(g => new { MatchId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.MatchId, x => x.Count);

                return Ok(unreadCounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading unread counts");
                return StatusCode(500, new { error = "Failed to load unread counts." });
            }
        }
    }
}
