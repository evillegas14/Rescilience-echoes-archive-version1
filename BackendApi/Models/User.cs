using System;
using System.ComponentModel.DataAnnotations;

namespace BackendApi.Models
{
    public class User
    {
        public int Id { get; set; } // Primary Key

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Store hash as string (e.g., Base64)

        [Required]
        public string PasswordSalt { get; set; } = string.Empty; // Store salt as string (e.g., Base64)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}