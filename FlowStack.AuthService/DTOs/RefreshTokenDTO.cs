using System.ComponentModel.DataAnnotations;

namespace FlowStack.AuthService.DTOs;

public class RefreshTokenDTO
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}