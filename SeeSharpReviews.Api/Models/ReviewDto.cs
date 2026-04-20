namespace SeeSharpReviews.Api.Models;

// Flat DTO sent over the wire. We don't send the EF Review entity directly
// because its User navigation would cause serialization loops.
public class ReviewDto
{
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string MovieApiId { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string MoviePosterPath { get; set; } = string.Empty;
    public string ReviewText { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
