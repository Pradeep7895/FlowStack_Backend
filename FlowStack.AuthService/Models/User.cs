using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.AuthService.Models;

public enum UserRole
{
    Member,
    BoardAdmin,
    PlatformAdmin
}

public enum OAuthProvider
{
    Local,
    Google,
    GitHub
}

[Table("Users")]
public class User
{
    [Key]
    public Guid UserId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    // Null for OAuth users who have no local password
    public string? PasswordHash { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Member;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    // Which OAuth provider was used (Local = email/password)
    public OAuthProvider Provider { get; set; } = OAuthProvider.Local;

    // Provider's own user id — used to match returning OAuth users
    public string? ProviderUserId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Refresh token support
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}