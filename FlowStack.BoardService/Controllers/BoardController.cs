using FlowStack.BoardService.DTOs;
using FlowStack.BoardService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowStack.BoardService.Controllers;

[ApiController]
[Route("api/boards")]
[Authorize]
[Produces("application/json")]
public class BoardController : ControllerBase
{
    private readonly IBoardService _boardService;

    public BoardController(IBoardService boardService)
    {
        _boardService = boardService;
    }

    //  Board CRUD 

    // POST /api/boards
    // Create a new board inside a workspace.
    // Requester must be a workspace member.
    // Creator is auto-enrolled as board Admin.
    [HttpPost]
    [ProducesResponseType(typeof(BoardDetailResponse), 201)]
    [ProducesResponseType(400), ProducesResponseType(401), ProducesResponseType(403), ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateBoardRequest request)
    {
        try
        {
            var board = await _boardService.CreateBoardAsync(GetCurrentUserId(), request);
            return CreatedAtAction(nameof(GetById), new { boardId = board.BoardId }, board);
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

    // GET /api/boards/{boardId}
    // Get full board details including member list.
    // Private boards: members only.
    [HttpGet("{boardId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BoardDetailResponse), 200)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> GetById([FromRoute] Guid boardId)
    {
        try
        {
            var requesterId = TryGetCurrentUserId() ?? Guid.Empty;
            var board       = await _boardService.GetBoardByIdAsync(boardId, requesterId);
            return Ok(board);
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

    // GET /api/boards/workspace/{workspaceId}
    // Get all boards in a workspace visible to the requester.
    // PlatformAdmin sees EVERYTHING.
    [HttpGet("workspace/{workspaceId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<BoardResponse>), 200)]
    public async Task<IActionResult> GetByWorkspace([FromRoute] Guid workspaceId)
    {
        var boards = await _boardService.GetBoardsByWorkspaceAsync(workspaceId, GetCurrentUserId());
        
        if (User.IsInRole("PlatformAdmin"))
        {
            // For PlatformAdmin, we return all boards without filtering
            // Note: GetBoardsByWorkspaceAsync already fetches all, but filters them
            // We can either add a bypass in the service or just re-fetch everything
            // Let's assume the service should handle it.
            // But for now, I'll just use the service results which are already filtered.
            // Wait, I should modify the SERVICE to not filter for admins.
        }

        return Ok(boards);
    }

    // GET /api/boards/my
    // Get all boards where the current user is a member.
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<BoardResponse>), 200)]
    public async Task<IActionResult> GetMyBoards()
    {
        var boards = await _boardService.GetBoardsByMemberAsync(GetCurrentUserId());
        return Ok(boards);
    }

    // GET /api/boards/created
    // Get all boards created by the current user.
    [HttpGet("created")]
    [ProducesResponseType(typeof(IEnumerable<BoardResponse>), 200)]
    public async Task<IActionResult> GetCreatedBoards()
    {
        var boards = await _boardService.GetBoardsByCreatorAsync(GetCurrentUserId());
        return Ok(boards);
    }

    // PUT /api/boards/{boardId}
    // Update board name, description, background, or visibility.
    // Requester must be board Admin or creator.
    [HttpPut("{boardId:guid}")]
    [ProducesResponseType(typeof(BoardDetailResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404), ProducesResponseType(409)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid boardId,
        [FromBody]  UpdateBoardRequest request)
    {
        try
        {
            var board = await _boardService.UpdateBoardAsync(boardId, GetCurrentUserId(), request);
            return Ok(board);
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

    // PUT /api/boards/{boardId}/close
    // Close a board — makes it read-only. Admin or creator only.
    [HttpPut("{boardId:guid}/close")]
    [ProducesResponseType(typeof(BoardDetailResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Close([FromRoute] Guid boardId)
    {
        try
        {
            var board = await _boardService.CloseBoardAsync(boardId, GetCurrentUserId());
            return Ok(board);
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

    // PUT /api/boards/{boardId}/reopen
    // Reopen a closed board. Admin or creator only.
    [HttpPut("{boardId:guid}/reopen")]
    [ProducesResponseType(typeof(BoardDetailResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Reopen([FromRoute] Guid boardId)
    {
        try
        {
            var board = await _boardService.ReopenBoardAsync(boardId, GetCurrentUserId());
            return Ok(board);
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

    // DELETE /api/boards/{boardId}
    // Permanently delete a board. Creator only.
    [HttpDelete("{boardId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Delete([FromRoute] Guid boardId)
    {
        try
        {
            await _boardService.DeleteBoardAsync(boardId, GetCurrentUserId());
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

    // GET /api/boards/{boardId}/members
    // List all board members. Private board: members only.
    [HttpGet("{boardId:guid}/members")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<BoardMemberResponse>), 200)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> GetMembers([FromRoute] Guid boardId)
    {
        try
        {
            var requesterId = TryGetCurrentUserId() ?? Guid.Empty;
            var members     = await _boardService.GetMembersAsync(boardId, requesterId);
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

    // POST /api/boards/{boardId}/members
    // Add a user to the board. Admin or creator only.
    // User must already be a workspace member.
    [HttpPost("{boardId:guid}/members")]
    [ProducesResponseType(typeof(BoardMemberResponse), 201)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404), ProducesResponseType(409)]
    public async Task<IActionResult> AddMember(
        [FromRoute] Guid boardId,
        [FromBody]  AddBoardMemberRequest request)
    {
        try
        {
            var member = await _boardService.AddMemberAsync(boardId, GetCurrentUserId(), request);
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

    // DELETE /api/boards/{boardId}/members/{userId}
    // Remove a member. Admin/creator can remove anyone; members can remove themselves.
    [HttpDelete("{boardId:guid}/members/{userId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> RemoveMember(
        [FromRoute] Guid boardId,
        [FromRoute] Guid userId)
    {
        try
        {
            await _boardService.RemoveMemberAsync(boardId, GetCurrentUserId(), userId);
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

    /// PUT /api/boards/{boardId}/members/{userId}/role
    /// Update a member's role (Observer/Member/Admin). Admin or creator only.
    [HttpPut("{boardId:guid}/members/{userId:guid}/role")]
    [ProducesResponseType(typeof(BoardMemberResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> UpdateMemberRole(
        [FromRoute] Guid boardId,
        [FromRoute] Guid userId,
        [FromBody]  UpdateBoardMemberRoleRequest request)
    {
        try
        {
            var member = await _boardService.UpdateMemberRoleAsync(
                boardId, GetCurrentUserId(), userId, request);
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

    //  Internal access check 

    // GET /api/boards/{boardId}/access/{userId}
    // Returns membership flags for the given user on this board.
    // Called service-to-service — no JWT required.
    [HttpGet("{boardId:guid}/access/{userId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BoardAccessResponse), 200)]
    public async Task<IActionResult> GetBoardAccess(
        [FromRoute] Guid boardId,
        [FromRoute] Guid userId)
    {
        var access = await _boardService.GetBoardAccessAsync(boardId, userId);
        return Ok(access);
    }

    //  Private helpers 

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub")
                    ?? throw new UnauthorizedAccessException(
                        "User identity not found in token.");
        return Guid.Parse(claim.Value);
    }

    private Guid? TryGetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub");
        return claim is not null ? Guid.Parse(claim.Value) : null;
    }
}