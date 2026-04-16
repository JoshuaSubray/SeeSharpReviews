using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeeSharpReviews.Data;
using SeeSharpReviews.Models;
using System.Security.Claims;

namespace SeeSharpReviews.Controllers;

[Authorize]
public class ReviewController : Controller
{
    private readonly AppDbContext _context;

    public ReviewController(AppDbContext context)
    {
        _context = context;
    }

    // get user's ID from claims.
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    // Check if the current user is an admin.
    private async Task<bool> IsCurrentUserAdminAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return false;

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Role?.RoleName == "Admin";
    }

    // GET: /Review/Create?...
    [HttpGet]
    public IActionResult Create(string movieApiId, string movieTitle, string moviePosterPath)
    {
        if (string.IsNullOrWhiteSpace(movieApiId))
            return BadRequest("Movie ID is required");

        var review = new Review
        {
            MovieApiId = movieApiId,
            MovieTitle = movieTitle ?? "Unknown Movie",
            MoviePosterPath = moviePosterPath ?? string.Empty
        };
        return View(review);
    }

    // POST: /Review/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Review review)
    {
        // Get current user's ID.
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            ModelState.AddModelError("", "You must be logged in to create a review.");
            return View(review);
        }

        // Load current user from database.
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            ModelState.AddModelError("", "User account not found.");
            return View(review);
        }

        // Remove all navigation property validation errors that can't be bound from forms.
        ModelState.Remove(nameof(review.User));
        ModelState.Remove($"{nameof(review.User)}.UserId");
        ModelState.Remove($"{nameof(review.User)}.Username");
        ModelState.Remove($"{nameof(review.User)}.Email");
        ModelState.Remove($"{nameof(review.User)}.PasswordHash");
        ModelState.Remove($"{nameof(review.User)}.RoleId");
        ModelState.Remove($"{nameof(review.User)}.Role");

        // attach user to review.
        review.User = user;
        review.UserId = userId;

        // validate required fields
        if (string.IsNullOrWhiteSpace(review.MovieApiId))
        {
            ModelState.AddModelError(nameof(review.MovieApiId), "Movie ID is required.");
            return View(review);
        }

        if (review.Rating < 1 || review.Rating > 5)
        {
            ModelState.AddModelError(nameof(review.Rating), "Please select a rating between 1 and 5.");
            return View(review);
        }

        if (string.IsNullOrWhiteSpace(review.ReviewText))
        {
            ModelState.AddModelError(nameof(review.ReviewText), "Review text is required.");
            return View(review);
        }

        if (!ModelState.IsValid)
        {
            return View(review);
        }

        // check if user has already reviewed this movie.
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieApiId == review.MovieApiId);

        if (existingReview != null)
        {
            ModelState.AddModelError("", "You have already reviewed this movie. Edit your existing review instead.");
            return View(review);
        }

        // create new review.
        var now = DateTime.UtcNow;
        var newReview = new Review
        {
            UserId = userId,
            User = user,
            MovieApiId = review.MovieApiId,
            MovieTitle = review.MovieTitle ?? "Unknown Movie",
            MoviePosterPath = review.MoviePosterPath ?? string.Empty,
            ReviewText = review.ReviewText.Trim(),
            Rating = review.Rating,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Reviews.Add(newReview);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Review created successfully!";
        return RedirectToAction("Details", "Movie", new { id = review.MovieApiId });
    }

    // GET: /Review/Edit/
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReviewId == id);
        if (review == null)
            return NotFound();

        var userId = GetCurrentUserId();
        var isAdmin = await IsCurrentUserAdminAsync();
        if (review.UserId != userId && !isAdmin)
            return Forbid(); // only the review owner or admin can edit it.

        return View(review);
    }

    // POST: /Review/Edit/
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Review review)
    {
        if (id != review.ReviewId)
            return NotFound();

        var existingReview = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReviewId == id);
        if (existingReview == null)
            return NotFound();

        var userId = GetCurrentUserId();
        var isAdmin = await IsCurrentUserAdminAsync();
        if (existingReview.UserId != userId && !isAdmin)
            return Forbid();

        // remove validation errors for fields that aren't in the form.
        // navigation properties can't be bound from forms.
        ModelState.Remove(nameof(review.User));
        ModelState.Remove($"{nameof(review.User)}.UserId");
        ModelState.Remove($"{nameof(review.User)}.Username");
        ModelState.Remove($"{nameof(review.User)}.Email");
        ModelState.Remove($"{nameof(review.User)}.PasswordHash");
        ModelState.Remove($"{nameof(review.User)}.RoleId");
        ModelState.Remove($"{nameof(review.User)}.Role");
        
        // fields not in the edit form.
        ModelState.Remove(nameof(review.MovieApiId));
        ModelState.Remove(nameof(review.MovieTitle));
        ModelState.Remove(nameof(review.MoviePosterPath));

        // validate rating.
        if (review.Rating < 1 || review.Rating > 5)
        {
            ModelState.AddModelError(nameof(review.Rating), "Please select a rating between 1 and 5.");
        }

        // validate review text.
        if (string.IsNullOrWhiteSpace(review.ReviewText))
        {
            ModelState.AddModelError(nameof(review.ReviewText), "Review text is required.");
        }

        if (!ModelState.IsValid)
            return View(existingReview);

        // update review.
        existingReview.ReviewText = review.ReviewText.Trim();
        existingReview.Rating = review.Rating;
        existingReview.UpdatedAt = DateTime.UtcNow;

        _context.Reviews.Update(existingReview);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Review updated successfully!";
        return RedirectToAction("Details", "Movie", new { id = existingReview.MovieApiId });
    }

    // GET: /Review/Delete/
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReviewId == id);
        if (review == null)
            return NotFound();

        var userId = GetCurrentUserId();
        var isAdmin = await IsCurrentUserAdminAsync();
        if (review.UserId != userId && !isAdmin)
            return Forbid();

        return View(review);
    }

    // POST: /Review/Delete/
    [HttpPost("Review/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReviewId == id);
        if (review == null)
            return NotFound();

        var userId = GetCurrentUserId();
        var isAdmin = await IsCurrentUserAdminAsync();
        if (review.UserId != userId && !isAdmin)
            return Forbid();

        var movieApiId = review.MovieApiId;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Review deleted successfully!";
        return RedirectToAction("Details", "Movie", new { id = movieApiId });
    }

    // GET: /Review/ByMovie/
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ByMovie(string movieApiId)
    {
        if (string.IsNullOrEmpty(movieApiId))
            return NotFound();

        var reviews = await _context.Reviews
            .Where(r => r.MovieApiId == movieApiId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        ViewBag.MovieApiId = movieApiId;
        return View(reviews);
    }
}
