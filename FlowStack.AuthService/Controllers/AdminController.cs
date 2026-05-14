using FlowStack.AuthService.DTOs;
using FlowStack.AuthService.Helpers;
using FlowStack.AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowStack.AuthService.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "PlatformAdmin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly RequestLogStore _logStore;

    public AdminController(IAuthService authService, RequestLogStore logStore)
    {
        _authService = authService;
        _logStore = logStore;
    }

    // GET /api/admin/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var users = await _authService.GetAllUsersAsync(page, pageSize);
        return Ok(users);
    }

    // GET /api/admin/users/{id}
    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetUserById(Guid userId)
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

    // POST /api/admin/users/{id}/promote
    [HttpPost("users/{userId:guid}/promote")]
    public async Task<IActionResult> PromoteToWorkspaceAdmin(Guid userId)
    {
        try
        {
            await _authService.PromoteToWorkspaceAdminAsync(userId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found." });
        }
    }

    // POST /api/admin/users/{id}/demote
    [HttpPost("users/{userId:guid}/demote")]
    public async Task<IActionResult> DemoteToMember(Guid userId)
    {
        try
        {
            await _authService.DemoteToMemberAsync(userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/admin/users/{id}/suspend
    [HttpPost("users/{userId:guid}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid userId)
    {
        try
        {
            await _authService.DeactivateAccountAsync(userId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/admin/users/{id}/reactivate
    [HttpPost("users/{userId:guid}/reactivate")]
    public async Task<IActionResult> ReactivateUser(Guid userId)
    {
        try
        {
            await _authService.ReactivateAccountAsync(userId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found." });
        }
    }

    // DELETE /api/admin/users/{id}
    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            await _authService.DeleteUserPermanentlyAsync(userId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/admin/logs
    [HttpGet("logs")]
    public IActionResult GetLogs()
    {
        return Ok(_logStore.GetLogs());
    }

    // GET /api/admin/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        return Ok(await _authService.GetPlatformStatsAsync());
    }
}
