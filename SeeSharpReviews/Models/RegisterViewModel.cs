using System.ComponentModel.DataAnnotations;

namespace SeeSharpReviews.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty; // Display name

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty; // Login email

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty; // Plain text password (hashed before saving)

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirm password do not match.")]
    public string ConfirmPassword { get; set; } = null!; // Must match password

    [Display(Name = "Register as Admin")]
    public bool IsAdminAccount { get; set; } // Grants admin privileges on creation
}
