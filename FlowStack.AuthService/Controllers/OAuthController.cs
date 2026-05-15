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
            RedirectUri = Url.Action(nameof(GoogleSuccess)),
            Items       = { ["returnUrl"] = returnUrl }
        };
        return Challenge(properties, "Google");
    }


    // GET /api/oauth/google/callback
    // ASP.NET Core Google middleware intercepts this path, exchanges the
    // authorization code for a Google access token, fetches the user profile,
    // and populates HttpContext.User — then execution reaches here.

    [HttpGet("google/success")]
    [ProducesResponseType(typeof(AuthResponseDTO), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GoogleSuccess()
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
            RedirectUri = Url.Action(nameof(GitHubSuccess)),
            Items       = { ["returnUrl"] = returnUrl }
        };
        return Challenge(properties, "GitHub");
    }


    // GET /api/oauth/github/callback
    // ASP.NET Core OAuth middleware (configured as "GitHub" in Program.cs)
    // intercepts this path, exchanges the code, fetches user profile from
    // api.github.com/user, maps claims, then execution reaches here.
    
    [HttpGet("github/success")]
    [ProducesResponseType(typeof(AuthResponseDTO), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GitHubSuccess()
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
        // Explicitly authenticate against the "External" cookie scheme
        var authResult = await HttpContext.AuthenticateAsync("External");
        
        if (!authResult.Succeeded)
        {
            return Unauthorized(new { message = "External authentication failed or session expired." });
        }

        var principal = authResult.Principal;
        var providerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email          = principal.FindFirst(ClaimTypes.Email)?.Value;
        var fullName       = principal.FindFirst(ClaimTypes.Name)?.Value;
        var avatarUrl      = principal.FindFirst("avatar_url")?.Value;

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

            // Instead of returning JSON, redirect back to the Frontend (port 3000)
            var frontendUrl = "http://localhost:3000/oauth-success";
            var query = $"?token={response.AccessToken}" +
                        $"&refreshToken={response.RefreshToken}" +
                        $"&userId={response.User.UserId}" +
                        $"&email={Uri.EscapeDataString(response.User.Email)}" +
                        $"&fullName={Uri.EscapeDataString(response.User.FullName)}" +
                        $"&username={Uri.EscapeDataString(response.User.Username)}" +
                        $"&role={response.User.Role}" +
                        $"&avatarUrl={Uri.EscapeDataString(response.User.AvatarUrl ?? "")}";

            return Redirect(frontendUrl + query);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Account exists but is deactivated
            return Redirect($"http://localhost:3000/login?error={Uri.EscapeDataString(ex.Message)}");
        }
    }
}