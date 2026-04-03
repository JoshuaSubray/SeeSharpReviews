using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeeSharpReviews.Models;

[Table("SSR_ROLES")]
public class Role
{
    [Key]
    public int RoleId { get; set; } // Primary key

    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty; // "Admin" or "User"

    public ICollection<User> Users { get; set; } = new List<User>(); // Users with this role
}
