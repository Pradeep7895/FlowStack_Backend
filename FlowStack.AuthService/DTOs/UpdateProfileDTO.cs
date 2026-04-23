using System.ComponentModel.DataAnnotations;

namespace FlowStack.AuthService.DTOs;

public class UpdateProfileDTO
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(50)]
    public string? Username { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }
}