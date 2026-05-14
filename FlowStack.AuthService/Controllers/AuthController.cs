using FlowStack.AuthService.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FlowStack.AuthService.Services;

namespace FlowStack.AuthService.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    //  Public endpoints 
    // POST /api/auth/register
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDTO), 201)]
    [ProducesResponseType(400), ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterDTO request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(GetProfile), new { }, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // POST /api/auth/login
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDTO), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginDTO request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // POST /api/auth/refresh — exchange refresh token for new access token
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDTO), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDTO request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // POST /api/auth/validate — called by other microservices to verify a JWT
    [HttpPost("validate")]
    [ProducesResponseType(typeof(TokenValidationResponseDTO), 200)]
    public async Task<IActionResult> Validate(
        [FromHeader(Name = "Authorization")] string authorization)
    {
        var token  = authorization?.Replace("Bearer ", "") ?? string.Empty;
        var result = await _authService.ValidateTokenAsync(token);
        return Ok(result);
    }

    // Authenticated endpoints 

    // POST /api/auth/logout — invalidate refresh token
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync(GetCurrentUserId());
        return NoContent();
    }

    // GET /api/auth/profile
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponseDTO), 200)]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _authService.GetUserByIdAsync(GetCurrentUserId());
        return Ok(user);
    }

    // PUT /api/auth/profile — update name, username, avatar, bio
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponseDTO), 200)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO request)
    {
        try
        {
            var user = await _authService.UpdateProfileAsync(GetCurrentUserId(), request);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT /api/auth/password — change password (local accounts only)
    [HttpPut("password")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO request)
    {
        try
        {
            await _authService.ChangePasswordAsync(GetCurrentUserId(), request);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // GET /api/auth/search?q=john — find users by name or username
    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<UserProfileResponseDTO>), 200)]
    public async Task<IActionResult> SearchUsers([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Search query cannot be empty." });

        var results = await _authService.SearchUsersAsync(q);
        return Ok(results);
    }

    // GET /api/auth/users/{id} — get any user by id (used by other services)
    [HttpGet("users/{userId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponseDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserById([FromRoute] Guid userId)
    {
        try
        {
            var user = await _authService.GetUserByIdAsync(userId);
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found." });
        }
    }

    /// DELETE /api/auth/deactivate — soft-deactivate own account
    [HttpDelete("deactivate")]
    [Authorize]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeactivateAccount()
    {
        await _authService.DeactivateAccountAsync(GetCurrentUserId());
        return NoContent();
    }

    // Private helpers 

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub")
                    ?? throw new UnauthorizedAccessException("User identity not found in token.");
        return Guid.Parse(claim.Value);
    }
}