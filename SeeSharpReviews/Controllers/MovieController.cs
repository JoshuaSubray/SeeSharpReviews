using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeeSharpReviews.Data;
using SeeSharpReviews.Models;
using SeeSharpReviews.Services;

namespace SeeSharpReviews.Controllers;

public class MovieController : Controller
{
    private readonly ITmdbApiService _tmdb;
    private readonly AppDbContext _context;

    public MovieController(ITmdbApiService tmdb, AppDbContext context)
    {
        _tmdb = tmdb;
        _context = context;
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

        var reviews = await _context.Reviews
            .Where(r => r.MovieApiId == id.ToString())
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        ViewBag.MovieReviews = reviews;
        ViewBag.MovieApiId = id;

        return View(movie);
    }
}
