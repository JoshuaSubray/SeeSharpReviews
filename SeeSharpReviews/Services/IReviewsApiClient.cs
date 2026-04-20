namespace SeeSharpReviews.Services;

public interface IReviewsApiClient
{
    Task<IReadOnlyList<ReviewDto>> GetReviewsAsync(int? take = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewDto>> GetReviewsByUserAsync(int userId, CancellationToken cancellationToken = default);
}

// Mirror of the API's ReviewDto. Lives inside the MVC project so we don't
// have to reference the Api project (which would create a weird dependency loop).
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
