using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeeSharpReviews.Data;
using SeeSharpReviews.Models;

namespace SeeSharpReviews.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Check if email is already taken
        var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (existingEmail != null)
        {
            ModelState.AddModelError("Email", "This email is already registered.");
            return View(model);
        }

        // Check if username is already taken
        var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
        if (existingUsername != null)
        {
            ModelState.AddModelError("Username", "This username is already taken.");
            return View(model);
        }

        // Get the requested role (Admin if checkbox checked, otherwise User)
        string requestedRole = model.IsAdminAccount ? "Admin" : "User";
        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == requestedRole); // Look up target role ID
        if (userRole == null)
        {
            ModelState.AddModelError("", "Registration is currently unavailable.");
            return View(model);
        }

        // Create new user with hashed password
        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            RoleId = userRole.RoleId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Find user by email and include their role
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        // Verify user exists and password is correct
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // Build claims for the cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // UserId
            new Claim(ClaimTypes.Name, user.Username),                    // Display name
            new Claim(ClaimTypes.Email, user.Email),                      // Email
            new Claim(ClaimTypes.Role, user.Role.RoleName)                // "Admin" or "User"
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Cookies", principal);

        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Logout
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Profile
    [HttpGet]
    public async Task<IActionResult> Profile(int? id)
    {
        User? user;

        if (id.HasValue)
        {
            // Viewing another user's profile
            user = await _context.Users
                .Include(u => u.Reviews)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id.Value);
        }
        else
        {
            // Viewing own profile (must be logged in)
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            user = await _context.Users
                .Include(u => u.Reviews)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        if (user == null)
            return NotFound();

        var model = new ProfileViewModel
        {
            Username = user.Username,
            RoleName = user.Role.RoleName,
            Reviews = user.Reviews.OrderByDescending(r => r.CreatedAt).ToList()
        };

        return View(model);
    }

    // POST: /Account/DeleteMyAccount
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteMyAccount()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId); // Find currently executing user
        
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            await HttpContext.SignOutAsync("Cookies"); // Burn the cookie
        }

        return RedirectToAction("Index", "Home");
    }
}
