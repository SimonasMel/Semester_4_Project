using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    /// <summary>
    /// Represents a mutual match between two users who have both liked each other's cars.
    /// Similar to a dating app match.
    /// </summary>
    public class MutualMatch
    {
        public string Id { get; set; }

        [Required]
        public string CurrentUserId { get; set; } = string.Empty;

        [Required]
        public string MatchedUserId { get; set; } = string.Empty;

        [Required]
        public string CurrentUserCarId { get; set; } = string.Empty;

        [Required]
        public string MatchedUserCarId { get; set; } = string.Empty;

        public DateTime MatchedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public MutualMatch()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
