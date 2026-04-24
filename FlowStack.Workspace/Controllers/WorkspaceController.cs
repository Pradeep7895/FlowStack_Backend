using FlowStack.Workspace.DTOs;
using FlowStack.Workspace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowStack.Workspace.Controllers;

[ApiController]
[Route("api/workspaces")]
[Authorize]
[Produces("application/json")]
public class WorkspaceController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspaceController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    // POST /api/workspaces
    // Create a new workspace. The requester is auto-enrolled as Admin member.
    [HttpPost]
    [ProducesResponseType(typeof(WorkspaceDetailResponse), 201)]
    [ProducesResponseType(400), ProducesResponseType(409)]
    public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceRequest request)
    {
        try
        {
            var workspace = await _workspaceService.CreateWorkspaceAsync(
                GetCurrentUserId(), request);
            return CreatedAtAction(
                nameof(GetById),
                new { workspaceId = workspace.WorkspaceId },
                workspace);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/workspaces/{workspaceId}
    // Get a workspace by ID including full member list
    [HttpGet("{workspaceId:guid}")]
    [AllowAnonymous]  
    [ProducesResponseType(typeof(WorkspaceDetailResponse), 200)]
    [ProducesResponseType(401), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> GetById([FromRoute] Guid workspaceId)
    {
        try
        {
            // Guests accessing public workspaces have no userId — use Guid.Empty as sentinel
            var requesterId = TryGetCurrentUserId() ?? Guid.Empty;
            var workspace   = await _workspaceService.GetByIdAsync(workspaceId, requesterId);
            return Ok(workspace);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }


    // GET /api/workspaces/my
    // Get all workspaces where the current user is the owner.
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceResponse>), 200)]
    public async Task<IActionResult> GetMyWorkspaces()
    {
        var workspaces = await _workspaceService.GetByOwnerAsync(GetCurrentUserId());
        return Ok(workspaces);
    }

    // GET /api/workspaces/member
    // Get all workspaces where the current user is a member (not necessarily owner).
    [HttpGet("member")]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceResponse>), 200)]
    public async Task<IActionResult> GetMemberWorkspaces()
    {
        var workspaces = await _workspaceService.GetByMemberAsync(GetCurrentUserId());
        return Ok(workspaces);
    }

    // GET /api/workspaces/public
    // Browse all public workspaces — accessible without login.
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceResponse>), 200)]
    public async Task<IActionResult> GetPublicWorkspaces()
    {
        var workspaces = await _workspaceService.GetPublicWorkspacesAsync();
        return Ok(workspaces);
    }

    // PUT /api/workspaces/{workspaceId}
    // Update workspace settings. Requester must be Admin or Owner.

    [HttpPut("{workspaceId:guid}")]
    [ProducesResponseType(typeof(WorkspaceDetailResponse), 200)]
    [ProducesResponseType(403), ProducesResponseType(404), ProducesResponseType(409)]
    public async Task<IActionResult> UpdateWorkspace(
        [FromRoute] Guid workspaceId,
        [FromBody]  UpdateWorkspaceRequest request)
    {
        try
        {
            var workspace = await _workspaceService.UpdateWorkspaceAsync(
                workspaceId, GetCurrentUserId(), request);
            return Ok(workspace);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DELETE /api/workspaces/{workspaceId}
    // Permanently delete a workspace. Owner only.
    [HttpDelete("{workspaceId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> DeleteWorkspace([FromRoute] Guid workspaceId)
    {
        try
        {
            await _workspaceService.DeleteWorkspaceAsync(workspaceId, GetCurrentUserId());
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    //  Member management 

    // GET /api/workspaces/{workspaceId}/members
    // List all members. Private workspace members are hidden from non-members.
    [HttpGet("{workspaceId:guid}/members")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceMemberResponse>), 200)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> GetMembers([FromRoute] Guid workspaceId)
    {
        try
        {
            var requesterId = TryGetCurrentUserId() ?? Guid.Empty;
            var members = await _workspaceService.GetMembersAsync(workspaceId, requesterId);
            return Ok(members);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // POST /api/workspaces/{workspaceId}/members
    // Add a user to the workspace. Requester must be Admin or Owner.
    [HttpPost("{workspaceId:guid}/members")]
    [ProducesResponseType(typeof(WorkspaceMemberResponse), 201)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404), ProducesResponseType(409)]
    public async Task<IActionResult> AddMember(
        [FromRoute] Guid workspaceId,
        [FromBody]  AddMemberRequest request)
    {
        try
        {
            var member = await _workspaceService.AddMemberAsync(
                workspaceId, GetCurrentUserId(), request);
            return StatusCode(201, member);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DELETE /api/workspaces/{workspaceId}/members/{userId}
    // Remove a member. Admins/Owner can remove anyone; members can remove themselves.
    [HttpDelete("{workspaceId:guid}/members/{userId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> RemoveMember(
        [FromRoute] Guid workspaceId,
        [FromRoute] Guid userId)
    {
        try
        {
            await _workspaceService.RemoveMemberAsync(workspaceId, GetCurrentUserId(), userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/workspaces/{workspaceId}/members/{userId}/role
    // Update a member's role. Requester must be Admin or Owner.
    [HttpPut("{workspaceId:guid}/members/{userId:guid}/role")]
    [ProducesResponseType(typeof(WorkspaceMemberResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> UpdateMemberRole(
        [FromRoute] Guid workspaceId,
        [FromRoute] Guid userId,
        [FromBody]  UpdateMemberRoleRequest request)
    {
        try
        {
            var member = await _workspaceService.UpdateMemberRoleAsync(
                workspaceId, GetCurrentUserId(), userId, request);
            return Ok(member);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    //  Internal membership check 

    /// GET /api/workspaces/{workspaceId}/members/{userId}/check
    /// Lightweight membership check called by board-service to validate access.
    /// Returns 200 with IsMember/IsAdmin flags — no auth required (service-to-service).

    [HttpGet("{workspaceId:guid}/members/{userId:guid}/check")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> CheckMembership(
        [FromRoute] Guid workspaceId,
        [FromRoute] Guid userId)
    {
        var isMember       = await _workspaceService.IsMemberAsync(workspaceId, userId);
        var isAdminOrOwner = await _workspaceService.IsAdminOrOwnerAsync(workspaceId, userId);
        return Ok(new { isMember, isAdminOrOwner });
    }

    //  Private helpers 

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub")
                    ?? throw new UnauthorizedAccessException("User identity not found in token.");
        return Guid.Parse(claim.Value);
    }

    // Returns null for anonymous (guest) requests.
    private Guid? TryGetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub");
        return claim is not null ? Guid.Parse(claim.Value) : null;
    }
}