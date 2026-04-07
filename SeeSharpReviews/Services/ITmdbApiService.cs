using SeeSharpReviews.Models;

namespace SeeSharpReviews.Services;

public interface ITmdbApiService
{
    Task<IReadOnlyList<Movie>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Movie>> DiscoverMoviesAsync(int? genreId, int? year, CancellationToken cancellationToken = default);

    Task<Movie?> GetMovieDetailsAsync(int tmdbId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TmdbGenreOption>> GetMovieGenresAsync(CancellationToken cancellationToken = default);

    bool IsApiKeyConfigured { get; }
}
