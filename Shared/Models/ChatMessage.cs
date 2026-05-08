using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    /// <summary>
    /// Represents a single chat message between two matched users.
    /// </summary>
    public class ChatMessage
    {
        public string Id { get; set; }

        [Required]
        public string MatchId { get; set; } = string.Empty;

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public ChatMessage()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
