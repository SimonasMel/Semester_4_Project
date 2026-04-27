using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System.Security.Claims;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MatchesController : ControllerBase
    {
        private readonly CarDbContext _db;
        private readonly ILogger<MatchesController> _logger;

        public MatchesController(CarDbContext db, ILogger<MatchesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("mutual")]
        public async Task<ActionResult<IEnumerable<MutualMatch>>> GetMutualMatches()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new { error = "User not logged in" });

                var matches = await _db.MutualMatches
                    .Where(m => m.IsActive && (m.CurrentUserId == userId || m.MatchedUserId == userId))
                    .OrderByDescending(m => m.MatchedDate)
                    .ToListAsync();

                return Ok(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mutual matches");
                return StatusCode(500, new { error = "An error occurred while retrieving mutual matches", details = ex.Message });
            }
        }

        [HttpDelete("mutual/{id}")]
        public async Task<IActionResult> RemoveMutualMatch(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest(new { error = "Match ID cannot be empty" });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new { error = "User not logged in" });

                var match = await _db.MutualMatches.FirstOrDefaultAsync(m => m.Id == id);
                if (match == null)
                    return NotFound(new { error = $"Match with ID {id} not found" });

                if (match.CurrentUserId != userId && match.MatchedUserId != userId)
                    return Forbid();

                match.IsActive = false;
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing mutual match {MatchId}", id);
                return StatusCode(500, new { error = "An error occurred while removing the mutual match", details = ex.Message });
            }
        }
    }
}
