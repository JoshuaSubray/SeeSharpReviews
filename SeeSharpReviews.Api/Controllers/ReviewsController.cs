using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeeSharpReviews.Api.Models;
using SeeSharpReviews.Data;

namespace SeeSharpReviews.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(AppDbContext db, ILogger<ReviewsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET /api/reviews
    // GET /api/reviews?take=20
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews(
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        var query = _db.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .AsQueryable();

        if (take is > 0)
            query = query.Take(take.Value);

        var reviews = await ProjectToDto(query).ToListAsync(cancellationToken);
        return Ok(reviews);
    }

    // GET /api/reviews/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReviewDto>> GetReview(int id, CancellationToken cancellationToken)
    {
        var review = await ProjectToDto(
                _db.Reviews.AsNoTracking()
                           .Include(r => r.User)
                           .Where(r => r.ReviewId == id))
            .FirstOrDefaultAsync(cancellationToken);

        return review is null ? NotFound() : Ok(review);
    }

    // GET /api/reviews/user/5
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByUser(
        int userId,
        CancellationToken cancellationToken)
    {
        var reviews = await ProjectToDto(
                _db.Reviews.AsNoTracking()
                           .Include(r => r.User)
                           .Where(r => r.UserId == userId)
                           .OrderByDescending(r => r.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(reviews);
    }

    private static IQueryable<ReviewDto> ProjectToDto(IQueryable<SeeSharpReviews.Models.Review> query) =>
        query.Select(r => new ReviewDto
        {
            ReviewId = r.ReviewId,
            UserId = r.UserId,
            Username = r.User.Username,
            MovieApiId = r.MovieApiId,
            MovieTitle = r.MovieTitle,
            MoviePosterPath = r.MoviePosterPath,
            ReviewText = r.ReviewText,
            Rating = r.Rating,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        });
}
