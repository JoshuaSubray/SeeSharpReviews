namespace SeeSharpReviews.Models;

public class ProfileViewModel
{
    public string Username { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public List<Review> Reviews { get; set; } = new List<Review>(); // All reviews written by this user
}
