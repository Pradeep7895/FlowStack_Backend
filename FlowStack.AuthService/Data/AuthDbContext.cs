using FlowStack.AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");

        // Seed PlatformAdmin
        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = Guid.Parse("adadadad-adad-adad-adad-adadadadadad"),
            FullName = "Platform Administrator",
            Email = "admin@flowstack.io",
            Username = "platform_admin",
            PasswordHash = "$2a$11$q8bNbKoL//FYPam5TB3lfOPsl6bR/mPdrOOfmlbRF7BSLmj40n.dy",
            Role = UserRole.PlatformAdmin,
            Provider = OAuthProvider.Local,
            IsActive = true,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        base.OnModelCreating(modelBuilder);
    }
}