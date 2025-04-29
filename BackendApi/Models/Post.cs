using System;
using System.ComponentModel.DataAnnotations;

namespace BackendApi.Models
{
    public class Post
    {
        [Key] // Specifies the primary key
        public int PostId { get; set; }

        [Required] // Makes Title mandatory
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime DatePublished { get; set; }

        public string? Timeline { get; set; } // Nullable string

        [Required]
        public string Story { get; set; } = string.Empty;

        public string? ImageURL { get; set; } // URL of the uploaded image in Blob Storage

        public string? WorkCited { get; set; } // Optional

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to current UTC time
    }
}
