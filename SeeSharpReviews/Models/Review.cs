using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeeSharpReviews.Models;

[Table("SSR_REVIEWS")]
public class Review
{
    [Key]
    public int ReviewId { get; set; } // Primary key

    [ForeignKey("User")]
    public int UserId { get; set; } // Foreign key to User table

    public User User { get; set; } = null!; // Navigation to User

    [Required]
    public string MovieApiId { get; set; } = string.Empty; // TMDb movie ID

    [Required]
    [MaxLength(255)]
    public string MovieTitle { get; set; } = string.Empty; // Stored to avoid re-fetching from API

    [MaxLength(500)]
    public string MoviePosterPath { get; set; } = string.Empty; // TMDb poster path, stored to avoid re-fetching

    [Required]
    [MaxLength(1000)]
    public string ReviewText { get; set; } = string.Empty; // The review content

    [Range(1, 5)]
    public int Rating { get; set; } // Star rating 1 to 5

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // When review was created

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // When review was last edited
}
