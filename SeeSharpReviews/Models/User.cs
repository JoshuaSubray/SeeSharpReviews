using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeeSharpReviews.Models;

public class User
{
    [Key]
    public int UserId { get; set; } // Primary key

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty; // Display name

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty; // Login email

    [Required]
    public string PasswordHash { get; set; } = string.Empty; // Hashed password

    [ForeignKey("Role")]
    public int RoleId { get; set; } // Foreign key to Role table

    public Role Role { get; set; } = null!; // Navigation to Role

    public ICollection<Review> Reviews { get; set; } = new List<Review>(); // Reviews written by this user
}
