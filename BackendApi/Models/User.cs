using System.ComponentModel.DataAnnotations;

namespace BackendApi.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Store hashed passwords only!

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
