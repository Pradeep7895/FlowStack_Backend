using FlowStack.AuthService.DTOs;
using FlowStack.AuthService.Models;

namespace FlowStack.AuthService.Services;

public interface IAuthService
{
    Task<AuthResponseDTO> RegisterAsync(RegisterDTO request);
    Task<AuthResponseDTO> LoginAsync(LoginDTO request);
    Task LogoutAsync(Guid userId);
    Task<AuthResponseDTO> RefreshTokenAsync(string refreshToken);

    //used by other services via /api/auth/validate
    Task<TokenValidationResponseDTO> ValidateTokenAsync(string token);

    // Profile management
    Task<UserProfileResponseDTO> GetUserByIdAsync(Guid userId);
    Task<UserProfileResponseDTO> GetUserByEmailAsync(string email);
    Task<UserProfileResponseDTO> UpdateProfileAsync(Guid userId, UpdateProfileDTO request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDTO request);
    Task DeactivateAccountAsync(Guid userId);
    Task ReactivateAccountAsync(Guid userId);

    // User search — for workspace/board member invitations
    Task<IEnumerable<UserProfileResponseDTO>> SearchUsersAsync(string query);

    // OAuth2 — called from the OAuth callback handler
    Task<AuthResponseDTO> HandleOAuthCallbackAsync(OAuthProvider provider, string providerUserId, string email, string fullName, string? avatarUrl);

    // Platform Admin operations
    Task DeleteUserPermanentlyAsync(Guid userId);
    Task<IEnumerable<UserProfileResponseDTO>> GetAllUsersAsync(int page, int pageSize);
}