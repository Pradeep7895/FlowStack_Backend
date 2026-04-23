using FlowStack.AuthService.DTOs;
using FlowStack.AuthService.Models;
using FlowStack.AuthService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowStack.AuthService.Controllers;

// Flow:
//   1. Client calls GET /api/oauth/{provider}/login
//      → redirected to provider's consent screen
//   2. Provider redirects back to GET /api/oauth/{provider}/callback
//      → ASP.NET Core OAuth middleware validates the code, fetches user profile,
//        and populates HttpContext.User with claims
//   3. Callback action calls IAuthService.HandleOAuthCallbackAsync()
//      → finds or creates a User record, issues a FlowStack JWT
//   4. Returns the same AuthResponse shape as a normal email/password login

[ApiController]
[Route("api/oauth")]
[Produces("application/json")]
public class OAuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public OAuthController(IAuthService authService)
    {
        _authService = authService;
    }

    //  Google 
    // GET /api/oauth/google/login
    // Redirects the browser to Google's OAuth2 consent screen.
    // The redirect_uri registered in Google Cloud Console must match
    // the CallbackPath configured in Program.cs (/api/oauth/google/callback).
    [HttpGet("google/login")]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback)),
            Items       = { ["returnUrl"] = returnUrl }
        };
        return Challenge(properties, "Google");
    }


    // GET /api/oauth/google/callback
    // ASP.NET Core Google middleware intercepts this path, exchanges the
    // authorization code for a Google access token, fetches the user profile,
    // and populates HttpContext.User — then execution reaches here.

    [HttpGet("google/callback")]
    [ProducesResponseType(typeof(AuthResponseDTO), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GoogleCallback()
    {
        return await HandleCallbackAsync(OAuthProvider.Google);
    }


    //  GitHub 
    // GET /api/oauth/github/login
    // Redirects the browser to GitHub's OAuth2 authorization page.
    // The redirect_uri registered in your GitHub OAuth App must match
    // the CallbackPath configured in Program.cs (/api/oauth/github/callback).
    [HttpGet("github/login")]
    public IActionResult GitHubLogin([FromQuery] string? returnUrl = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GitHubCallback)),
            Items       = { ["returnUrl"] = returnUrl }
        };
        return Challenge(properties, "GitHub");
    }


    // GET /api/oauth/github/callback
    // ASP.NET Core OAuth middleware (configured as "GitHub" in Program.cs)
    // intercepts this path, exchanges the code, fetches user profile from
    // api.github.com/user, maps claims, then execution reaches here.
    
    [HttpGet("github/callback")]
    [ProducesResponseType(typeof(AuthResponseDTO), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GitHubCallback()
    {
        return await HandleCallbackAsync(OAuthProvider.GitHub);
    }

    //  Shared callback handler 
    // By the time this runs, ASP.NET Core OAuth middleware has already:
    //   - Validated the state parameter
    //   - Exchanged the code for an access token
    //   - Fetched the user profile and mapped it to claims on HttpContext.User
    // We just read those claims and hand them to the service layer.

    private async Task<IActionResult> HandleCallbackAsync(OAuthProvider provider)
    {
        // These claims are populated by the OAuth middleware — not from a JWT
        var providerUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email          = User.FindFirst(ClaimTypes.Email)?.Value;
        var fullName       = User.FindFirst(ClaimTypes.Name)?.Value;
        var avatarUrl      = User.FindFirst("avatar_url")?.Value;

        if (string.IsNullOrWhiteSpace(providerUserId) || string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new
            {
                message = $"Required claims (id, email) were not returned by {provider}. " +
                            "Ensure the correct scopes are requested."
            });
        }

        try
        {
            var response = await _authService.HandleOAuthCallbackAsync(
                provider,
                providerUserId,
                email,
                fullName ?? email.Split('@')[0],  
                avatarUrl);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Account exists but is deactivated
            return Unauthorized(new { message = ex.Message });
        }
    }
}