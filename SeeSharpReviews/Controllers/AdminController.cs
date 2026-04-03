using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeeSharpReviews.Data;
using SeeSharpReviews.Models;

namespace SeeSharpReviews.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Admin/Dashboard
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var users = await _context.Users
            .Include(u => u.Role) // Need role for the table
            .Include(u => u.Reviews) // Need reviews to count them
            .ToListAsync();

        var totalReviews = await _context.Reviews.CountAsync();

        var model = new AdminDashboardViewModel
        {
            TotalUsers = users.Count,
            TotalReviews = totalReviews,
            Users = users
        };

        return View(model);
    }

    // POST: /Admin/DeleteUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId); // Find the user to delete
        
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Dashboard");
    }

    // POST: /Admin/MakeAdmin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeAdmin(int userId)
    {
        var user = await _context.Users.FindAsync(userId); // Find the target user
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin"); // Get the internal admin role ID
        
        if (user != null && adminRole != null)
        {
            user.RoleId = adminRole.RoleId;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Dashboard");
    }
}
