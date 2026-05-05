using FlowStack.ListService.DTOs;
using FlowStack.ListService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowStack.ListService.Controllers;

[ApiController]
[Route("api/lists")]
[Authorize]
[Produces("application/json")]
public class ListController : ControllerBase
{
    private readonly IListService _listService;

    public ListController(IListService listService)
    {
        _listService = listService;
    }

    // Create 

    // POST /api/lists
    // Create a new list on a board.
    // Position is auto-assigned as max+1
    [HttpPost]
    [ProducesResponseType(typeof(TaskListResponse), 201)]
    [ProducesResponseType(400), ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] CreateListRequest request)
    {
        try
        {
            var list = await _listService.CreateListAsync(GetCurrentUserId(), request);
            return CreatedAtAction(nameof(GetById), new { listId = list.ListId }, list);
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

    // Read 

    // GET /api/lists/{listId}
    // Get a single list by its ID.
    [HttpGet("{listId:guid}")]
    [ProducesResponseType(typeof(TaskListResponse), 200)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> GetById([FromRoute] Guid listId)
    {
        try
        {
            var list = await _listService.GetListByIdAsync(listId, GetCurrentUserId());
            return Ok(list);
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


    // GET /api/lists/board/{boardId}
    // Get all active lists for a board, ordered by position.
    [HttpGet("board/{boardId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TaskListResponse>), 200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetByBoard([FromRoute] Guid boardId)
    {
        try
        {
            var lists = await _listService.GetListsByBoardAsync(boardId, GetCurrentUserId());
            return Ok(lists);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    /// GET /api/lists/board/{boardId}/archived
    /// Get all archived lists for a board.
    /// Used in the "View Archived Items" panel.
    [HttpGet("board/{boardId:guid}/archived")]
    [ProducesResponseType(typeof(IEnumerable<TaskListResponse>), 200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetArchivedByBoard([FromRoute] Guid boardId)
    {
        try
        {
            var lists = await _listService.GetArchivedListsAsync(boardId, GetCurrentUserId());
            return Ok(lists);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // Update 

    // PUT /api/lists/{listId}
    // Update list name or colour.
    [HttpPut("{listId:guid}")]
    [ProducesResponseType(typeof(TaskListResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid listId,
        [FromBody]  UpdateListRequest request)
    {
        try
        {
            var list = await _listService.UpdateListAsync(listId, GetCurrentUserId(), request);
            return Ok(list);
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

    // PUT /api/lists/reorder
    // Reorder all active lists on a board after a drag-and-drop.
    [HttpPut("reorder")]
    [ProducesResponseType(typeof(IEnumerable<TaskListResponse>), 200)]
    [ProducesResponseType(400), ProducesResponseType(403)]
    public async Task<IActionResult> Reorder([FromBody] ReorderListsRequest request)
    {
        try
        {
            var lists = await _listService.ReorderListsAsync(GetCurrentUserId(), request);
            return Ok(lists);
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

    /// PUT /api/lists/{listId}/move
    /// Move a list to a different board within the same workspace.
    [HttpPut("{listId:guid}/move")]
    [ProducesResponseType(typeof(TaskListResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Move([FromRoute] Guid listId, [FromBody]  MoveListRequest request)
    {
        try
        {
            var list = await _listService.MoveListAsync(listId, GetCurrentUserId(), request);
            return Ok(list);
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

    // Archive / Unarchive 

    // POST /api/lists/{listId}/archive
    // Soft-archive a list — hides it from the board view.
    // Cards inside remain intact and are recoverable.
    [HttpPost("{listId:guid}/archive")]
    [ProducesResponseType(typeof(TaskListResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Archive([FromRoute] Guid listId)
    {
        try
        {
            var list = await _listService.ArchiveListAsync(listId, GetCurrentUserId());
            return Ok(list);
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

    // POST /api/lists/{listId}/unarchive
    // Restore an archived list — appended to the end of active lists.
    [HttpPost("{listId:guid}/unarchive")]
    [ProducesResponseType(typeof(TaskListResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Unarchive([FromRoute] Guid listId)
    {
        try
        {
            var list = await _listService.UnarchiveListAsync(listId, GetCurrentUserId());
            return Ok(list);
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

    //  Hard delete 

    /// DELETE /api/lists/{listId}
    /// Permanently delete an archived list.
    /// The list must be archived first — active lists cannot be hard-deleted.
    /// Board Admin or creator only.
    [HttpDelete("{listId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Delete([FromRoute] Guid listId)
    {
        try
        {
            await _listService.DeleteListAsync(listId, GetCurrentUserId());
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

    // Private helpers 

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub")
                    ?? throw new UnauthorizedAccessException("User identity not found in token.");
        return Guid.Parse(claim.Value);
    }
}