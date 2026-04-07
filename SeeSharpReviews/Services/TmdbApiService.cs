using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SeeSharpReviews.Models;

namespace SeeSharpReviews.Services;

public class TmdbApiService : ITmdbApiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TmdbApiService> _logger;

    public TmdbApiService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<TmdbApiService> logger)
    {
        _http = httpClientFactory.CreateClient("TMDb");
        _configuration = configuration;
        _logger = logger;
    }

    private string? ApiKey => _configuration["TMDb:ApiKey"];

    public bool IsApiKeyConfigured => !string.IsNullOrWhiteSpace(ApiKey);

    public async Task<IReadOnlyList<Movie>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default)
    {
        if (!IsApiKeyConfigured || string.IsNullOrWhiteSpace(query))
            return Array.Empty<Movie>();

        var q = Uri.EscapeDataString(query.Trim());
        var url = $"search/movie?query={q}&include_adult=false";
        var payload = await GetAsync<TmdbPagedResults<TmdbMovieListItem>>(url, cancellationToken);
        if (payload?.Results == null)
            return Array.Empty<Movie>();

        return payload.Results.Select(ToListMovie).ToList();
    }

    public async Task<IReadOnlyList<Movie>> DiscoverMoviesAsync(int? genreId, int? year, CancellationToken cancellationToken = default)
    {
        if (!IsApiKeyConfigured)
            return Array.Empty<Movie>();

        var qs = new List<string>
        {
            "sort_by=popularity.desc",
            "include_adult=false"
        };

        if (genreId is int g && g > 0)
            qs.Add($"with_genres={g}");

        if (year is int y && y > 0)
            qs.Add($"primary_release_year={y}");

        var url = "discover/movie?" + string.Join('&', qs);
        var payload = await GetAsync<TmdbPagedResults<TmdbMovieListItem>>(url, cancellationToken);
        if (payload?.Results == null)
            return Array.Empty<Movie>();

        return payload.Results.Select(ToListMovie).ToList();
    }

    public async Task<Movie?> GetMovieDetailsAsync(int tmdbId, CancellationToken cancellationToken = default)
    {
        if (!IsApiKeyConfigured)
            return null;

        var url = $"movie/{tmdbId}";
        var detail = await GetAsync<TmdbMovieDetail>(url, cancellationToken);
        if (detail == null)
            return null;

        return new Movie
        {
            Id = detail.Id,
            Title = detail.Title ?? string.Empty,
            Tagline = detail.Tagline ?? string.Empty,
            Overview = detail.Overview ?? string.Empty,
            PosterPath = detail.PosterPath ?? string.Empty,
            ReleaseDate = detail.ReleaseDate ?? string.Empty,
            Runtime = detail.Runtime ?? 0,
            VoteAverage = detail.VoteAverage,
            Status = detail.Status ?? string.Empty,
            Genres = detail.Genres?.Select(g => g.Name).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>()
        };
    }

    public async Task<IReadOnlyList<TmdbGenreOption>> GetMovieGenresAsync(CancellationToken cancellationToken = default)
    {
        if (!IsApiKeyConfigured)
            return Array.Empty<TmdbGenreOption>();

        var payload = await GetAsync<TmdbGenreListResponse>("genre/movie/list", cancellationToken);
        if (payload?.Genres == null)
            return Array.Empty<TmdbGenreOption>();

        return payload.Genres
            .Select(g => new TmdbGenreOption { Id = g.Id, Name = g.Name ?? string.Empty })
            .OrderBy(g => g.Name)
            .ToList();
    }

    private async Task<T?> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken) where T : class
    {
        if (!IsApiKeyConfigured || string.IsNullOrWhiteSpace(ApiKey))
            return null;

        try
        {
            var sep = relativeUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
            var requestUrl = $"{relativeUrl}{sep}api_key={Uri.EscapeDataString(ApiKey)}";
            var response = await _http.GetAsync(requestUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TMDb request failed: {Status} {Url}", response.StatusCode, requestUrl);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "TMDb request error for {Url}", relativeUrl);
            return null;
        }
    }

    private static Movie ToListMovie(TmdbMovieListItem m) =>
        new()
        {
            Id = m.Id,
            Title = m.Title ?? string.Empty,
            Overview = m.Overview ?? string.Empty,
            PosterPath = m.PosterPath ?? string.Empty,
            ReleaseDate = m.ReleaseDate ?? string.Empty,
            VoteAverage = m.VoteAverage
        };

    private sealed class TmdbPagedResults<TItem>
    {
        [JsonPropertyName("results")]
        public List<TItem>? Results { get; set; }
    }

    private sealed class TmdbMovieListItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }
    }

    private sealed class TmdbMovieDetail
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("tagline")]
        public string? Tagline { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("runtime")]
        public int? Runtime { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("genres")]
        public List<TmdbGenreItem>? Genres { get; set; }
    }

    private sealed class TmdbGenreItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class TmdbGenreListResponse
    {
        [JsonPropertyName("genres")]
        public List<TmdbGenreItem>? Genres { get; set; }
    }
}
