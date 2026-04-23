namespace FlowStack.AuthService.DTOs;

public class TokenValidationResponseDTO
{
    public bool IsValid { get; set; }
    public Guid? UserId { get; set; }
    public string? Role { get; set; }
    public string? Email { get; set; }
}