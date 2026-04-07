namespace SeeSharpReviews.Models;

public class MovieIndexViewModel
{
    public string? Query { get; set; }
    public int? GenreId { get; set; }
    public int? Year { get; set; }
    public IReadOnlyList<Movie> Results { get; set; } = Array.Empty<Movie>();
    public IReadOnlyList<TmdbGenreOption> Genres { get; set; } = Array.Empty<TmdbGenreOption>();
    public bool ApiKeyMissing { get; set; }
    public string? ErrorMessage { get; set; }
}
