using FlowStack.AuthService.DTOs;
using FlowStack.AuthService.Helpers;
using FlowStack.AuthService.Models;
using FlowStack.AuthService.Repositories;

namespace FlowStack.AuthService.Services;

public class AuthServiceImpl : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly JwtHelper _jwt;

    public AuthServiceImpl(IUserRepository userRepo, JwtHelper jwt)
    {
        _userRepo = userRepo;
        _jwt = jwt;
    }

    // Registration 

    public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO request)
    {
        if (await _userRepo.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException("An account with this email already exists.");

        if (await _userRepo.ExistsByUsernameAsync(request.Username))
            throw new InvalidOperationException("This username is already taken.");

        var refreshToken = _jwt.GenerateRefreshToken();

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            Username = request.Username.ToLower(),

            // BCrypt automatically salts and hashes the password
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Provider = OAuthProvider.Local,
            Role = UserRole.Member,
            RefreshToken = refreshToken,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(30)
        };

        user = await _userRepo.CreateAsync(user);
        var accessToken = _jwt.GenerateAccessToken(user);

        return BuildAuthResponse(user, accessToken, refreshToken);
    }

    // Login 

    public async Task<AuthResponseDTO> LoginAsync(LoginDTO request)
    {
        var user = await _userRepo.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated.");

        // Here BCrypt.Verify re-hashes the input and compares — password is never stored plain
        if (user.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var refreshToken = _jwt.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        await _userRepo.UpdateAsync(user);

        var accessToken = _jwt.GenerateAccessToken(user);
        return BuildAuthResponse(user, accessToken, refreshToken);
    }

    //  Logout 

    public async Task LogoutAsync(Guid userId)
    {
        var user = await RequireUserAsync(userId);

        // Invalidate refresh token so it can't be used to get new access tokens
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userRepo.UpdateAsync(user);
    }

    //  Token refresh 
    public async Task<AuthResponseDTO> RefreshTokenAsync(string refreshToken)
    {
        // Find the user who owns this refresh token
        var allUsers = await _userRepo.GetAllUsersAsync(1, int.MaxValue);
        var user = allUsers.FirstOrDefault(u =>
            u.RefreshToken == refreshToken &&
            u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        // Rotate the refresh token on every use (one-time use pattern)
        var newRefreshToken = _jwt.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        await _userRepo.UpdateAsync(user);

        var accessToken = _jwt.GenerateAccessToken(user);
        return BuildAuthResponse(user, accessToken, newRefreshToken);
    }

    //  Token validation 
    public Task<TokenValidationResponseDTO> ValidateTokenAsync(string token)
    {
        var principal = _jwt.ValidateToken(token);

        if (principal is null)
            return Task.FromResult(new TokenValidationResponseDTO { IsValid = false });

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
        var roleClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.Role);
        var emailClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.Email) ?? principal.FindFirst("email");

        return Task.FromResult(new TokenValidationResponseDTO
        {
            IsValid = true,
            UserId = userIdClaim is not null ? Guid.Parse(userIdClaim.Value) : null,
            Role = roleClaim?.Value,
            Email = emailClaim?.Value
        });
    }

    //  Profile management 
    public async Task<UserProfileResponseDTO> GetUserByIdAsync(Guid userId)
    {
        var user = await RequireUserAsync(userId);
        return UserProfileResponseDTO.FromUser(user);
    }

    public async Task<UserProfileResponseDTO> GetUserByEmailAsync(string email)
    {
        var user = await _userRepo.FindByEmailAsync(email)
            ?? throw new KeyNotFoundException($"No user found with email: {email}");
        return UserProfileResponseDTO.FromUser(user);
    }

    public async Task<UserProfileResponseDTO> UpdateProfileAsync(Guid userId, UpdateProfileDTO request)
    {
        var user = await RequireUserAsync(userId);

        // Only update fields that were actually provided
        if (request.FullName is not null)
            user.FullName = request.FullName;

        if (request.Username is not null)
        {
            if (await _userRepo.ExistsByUsernameAsync(request.Username) &&
                user.Username != request.Username.ToLower())
                throw new InvalidOperationException("Username is already taken.");

            user.Username = request.Username.ToLower();
        }

        if (request.AvatarUrl is not null)
            user.AvatarUrl = request.AvatarUrl;

        if (request.Bio is not null)
            user.Bio = request.Bio;

        user = await _userRepo.UpdateAsync(user);
        return UserProfileResponseDTO.FromUser(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDTO request)
    {
        var user = await RequireUserAsync(userId);

        if (user.Provider != OAuthProvider.Local)
            throw new InvalidOperationException("OAuth users do not have a local password.");

        if (user.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepo.UpdateAsync(user);
    }

    public async Task DeactivateAccountAsync(Guid userId)
    {
        if (userId == Guid.Parse("adadadad-adad-adad-adad-adadadadadad"))
            throw new InvalidOperationException("Cannot deactivate the root platform administrator.");

        var user = await RequireUserAsync(userId);
        user.IsActive = false;
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userRepo.UpdateAsync(user);
    }

    public async Task ReactivateAccountAsync(Guid userId)
    {
        // Must bypass query filter to find inactive users
        var user = await _userRepo.FindByUserIdIncludingInactiveAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");
        user.IsActive = true;
        await _userRepo.UpdateAsync(user);
    }

    //  User search 

    public async Task<IEnumerable<UserProfileResponseDTO>> SearchUsersAsync(string query)
    {
        var users = await _userRepo.SearchByFullNameAsync(query);
        return users.Select(UserProfileResponseDTO.FromUser);
    }

    //  OAuth2 callback handler 
    public async Task<AuthResponseDTO> HandleOAuthCallbackAsync(
        OAuthProvider provider,
        string providerUserId,
        string email,
        string fullName,
        string? avatarUrl)
    {
        try
        {
            // Try to find an existing user linked to this OAuth account
            var user = await _userRepo.FindByProviderAsync(provider, providerUserId);

            if (user is null)
            {
                // Check if a local account exists with the same email — link it
                user = await _userRepo.FindByEmailAsync(email);

                if (user is not null)
                {
                    // Link the existing account to this OAuth provider
                    user.Provider = provider;
                    user.ProviderUserId = providerUserId;
                    if (avatarUrl is not null) user.AvatarUrl = avatarUrl;
                    user = await _userRepo.UpdateAsync(user);
                }
                else
                {
                    // Brand new user — create from OAuth profile
                    var baseUsername = email.Split('@')[0].ToLower();
                    var username = await EnsureUniqueUsernameAsync(baseUsername);

                    user = new User
                    {
                        FullName = fullName,
                        Email = email.ToLower(),
                        Username = username,
                        Provider = provider,
                        ProviderUserId = providerUserId,
                        AvatarUrl = avatarUrl,
                        Role = UserRole.Member
                    };
                    user = await _userRepo.CreateAsync(user);
                }
            }

            if (!user.IsActive)
                throw new UnauthorizedAccessException("This account has been deactivated.");

            var refreshToken = _jwt.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
            await _userRepo.UpdateAsync(user);

            var accessToken = _jwt.GenerateAccessToken(user);
            return BuildAuthResponse(user, accessToken, refreshToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    //  Platform Admin 

    public async Task DeleteUserPermanentlyAsync(Guid userId)
    {
        if (userId == Guid.Parse("adadadad-adad-adad-adad-adadadadadad"))
            throw new InvalidOperationException("Cannot delete the root platform administrator.");

        await _userRepo.DeleteByUserIdAsync(userId);
    }

    public async Task<IEnumerable<UserProfileResponseDTO>> GetAllUsersAsync(int page, int pageSize)
    {
        var users = await _userRepo.GetAllUsersAsync(page, pageSize);
        return users.Select(UserProfileResponseDTO.FromUser);
    }

    public async Task PromoteToWorkspaceAdminAsync(Guid userId)
    {
        var user = await RequireUserAsync(userId);
        user.Role = UserRole.WorkspaceAdmin;
        await _userRepo.UpdateAsync(user);
    }

    public async Task DemoteToMemberAsync(Guid userId)
    {
        if (userId == Guid.Parse("adadadad-adad-adad-adad-adadadadadad"))
            throw new InvalidOperationException("Cannot demote the root platform administrator.");

        var user = await RequireUserAsync(userId);
        user.Role = UserRole.Member;
        await _userRepo.UpdateAsync(user);
    }

    public async Task<object> GetPlatformStatsAsync()
    {
        var allUsers = await _userRepo.GetAllUsersAsync(1, int.MaxValue);

        return new
        {
            TotalUsers = allUsers.Count(),
            ActiveUsers = allUsers.Count(u => u.IsActive),
            PlatformAdmins = allUsers.Count(u => u.Role == UserRole.PlatformAdmin),
            WorkspaceAdmins = allUsers.Count(u => u.Role == UserRole.WorkspaceAdmin),
            Members = allUsers.Count(u => u.Role == UserRole.Member)
        };
    }

    //  Private helpers 

    private async Task<User> RequireUserAsync(Guid userId) =>
        await _userRepo.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

    private async Task<string> EnsureUniqueUsernameAsync(string base_)
    {
        var candidate = base_;
        var counter = 1;
        while (await _userRepo.ExistsByUsernameAsync(candidate))
            candidate = $"{base_}{counter++}";
        return candidate;
    }

    private static AuthResponseDTO BuildAuthResponse(User user, string accessToken, string refreshToken) => new()
    {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddHours(24),
        User = UserProfileResponseDTO.FromUser(user)
    };
}