using Microsoft.EntityFrameworkCore;
using SeeSharpReviews.Models;

namespace SeeSharpReviews.Data;

public class AppDbContext : DbContext
{
    // Receives DB connection settings from Program.cs
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } // Maps to Users table
    public DbSet<Role> Roles { get; set; } // Maps to Roles table
    public DbSet<Review> Reviews { get; set; } // Maps to Reviews table

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // One Role has many Users
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        // One User has many Reviews
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId);

        // Seed default roles
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "User" }
        );
    }
}
