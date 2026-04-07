using Microsoft.AspNetCore.Mvc;
using SeeSharpReviews.Models;
using SeeSharpReviews.Services;

namespace SeeSharpReviews.Controllers;

public class MovieController : Controller
{
    private readonly ITmdbApiService _tmdb;

    public MovieController(ITmdbApiService tmdb)
    {
        _tmdb = tmdb;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int? genreId, int? year, CancellationToken cancellationToken)
    {
        var genres = await _tmdb.GetMovieGenresAsync(cancellationToken);
        var vm = new MovieIndexViewModel
        {
            Query = q,
            GenreId = genreId,
            Year = year,
            Genres = genres,
            ApiKeyMissing = !_tmdb.IsApiKeyConfigured
        };

        if (!_tmdb.IsApiKeyConfigured)
            return View(vm);

        IReadOnlyList<Movie> results;
        if (!string.IsNullOrWhiteSpace(q))
            results = await _tmdb.SearchMoviesAsync(q, cancellationToken);
        else
            results = await _tmdb.DiscoverMoviesAsync(genreId, year, cancellationToken);

        vm.Results = results;
        if (results.Count == 0 && !vm.ApiKeyMissing)
            vm.ErrorMessage = string.IsNullOrWhiteSpace(q)
                ? "No movies matched these filters."
                : "No movies matched your search.";

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        if (!_tmdb.IsApiKeyConfigured)
        {
            ViewBag.ApiKeyMissing = true;
            return View((Movie?)null);
        }

        var movie = await _tmdb.GetMovieDetailsAsync(id, cancellationToken);
        if (movie == null)
            return NotFound();

        return View(movie);
    }
}
