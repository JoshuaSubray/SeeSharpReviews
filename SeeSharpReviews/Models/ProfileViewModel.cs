using SeeSharpReviews.Services;

namespace SeeSharpReviews.Models;

public class ProfileViewModel
{
    public string Username { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>(); // Fetched from the Reviews microservice
}
