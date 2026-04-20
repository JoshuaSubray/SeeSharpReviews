using System.Net.Http.Json;

namespace SeeSharpReviews.Services;

public class ReviewsApiClient : IReviewsApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ReviewsApiClient> _logger;

    public ReviewsApiClient(HttpClient http, ILogger<ReviewsApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public Task<IReadOnlyList<ReviewDto>> GetReviewsAsync(int? take = null, CancellationToken cancellationToken = default)
    {
        var url = take is > 0 ? $"api/reviews?take={take}" : "api/reviews";
        return GetListAsync(url, cancellationToken);
    }

    public Task<IReadOnlyList<ReviewDto>> GetReviewsByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return GetListAsync($"api/reviews/user/{userId}", cancellationToken);
    }

    // Shared helper so one bad response doesn't crash the MVC page.
    private async Task<IReadOnlyList<ReviewDto>> GetListAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<ReviewDto>>(url, cancellationToken);
            return result ?? new List<ReviewDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch reviews from the Reviews API at {Url}", _http.BaseAddress);
            return Array.Empty<ReviewDto>();
        }
    }
}
