using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class CarLike
    {
        public string Id { get; set; }

        [Required]
        public string LikerUserId { get; set; } = string.Empty;

        [Required]
        public string LikedCarId { get; set; } = string.Empty;

        [Required]
        public string LikedCarOwnerId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CarLike()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
