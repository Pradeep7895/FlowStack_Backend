using FlowStack.AuthService.Models;

namespace FlowStack.AuthService.DTOs;

public class UserProfileResponseDTO
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string Provider { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public static UserProfileResponseDTO FromUser(User user) => new()
    {
        UserId    = user.UserId,
        FullName  = user.FullName,
        Email     = user.Email,
        Username  = user.Username,
        Role      = user.Role.ToString(),
        AvatarUrl = user.AvatarUrl,
        Bio       = user.Bio,
        Provider  = user.Provider.ToString(),
        IsActive  = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}