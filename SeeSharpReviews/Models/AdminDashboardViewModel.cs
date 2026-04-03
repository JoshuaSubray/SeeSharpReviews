namespace SeeSharpReviews.Models;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; } // Count of all registered users
    public int TotalReviews { get; set; } // Count of all reviews globally
    public List<User> Users { get; set; } = new List<User>(); // List of users for the directory table
}
