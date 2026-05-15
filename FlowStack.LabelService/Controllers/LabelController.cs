using FlowStack.LabelService.DTOs;
using FlowStack.LabelService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowStack.LabelService.Controllers;

[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public class LabelController : ControllerBase
{
    private readonly ILabelService _labelService;

    public LabelController(ILabelService labelService)
    {
        _labelService = labelService;
    }

    // Labels 

    [HttpPost("labels")]
    [ProducesResponseType(typeof(LabelResponse), 201)]
    public async Task<IActionResult> CreateLabel([FromBody] CreateLabelRequest request)
    {
        try
        {
            var result = await _labelService.CreateLabelAsync(GetCurrentUserId(), GetAuthHeader(), request);
            return StatusCode(201, result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("boards/{boardId:guid}/labels")]
    [ProducesResponseType(typeof(IEnumerable<LabelResponse>), 200)]
    public async Task<IActionResult> GetLabelsByBoard([FromRoute] Guid boardId)
    {
        try
        {
            var result = await _labelService.GetLabelsByBoardAsync(boardId, GetCurrentUserId());
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpPut("labels/{labelId:guid}")]
    [ProducesResponseType(typeof(LabelResponse), 200)]
    public async Task<IActionResult> UpdateLabel([FromRoute] Guid labelId, [FromBody] UpdateLabelRequest request)
    {
        try
        {
            var result = await _labelService.UpdateLabelAsync(labelId, GetCurrentUserId(), GetAuthHeader(), request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("labels/{labelId:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteLabel([FromRoute] Guid labelId)
    {
        try
        {
            await _labelService.DeleteLabelAsync(labelId, GetCurrentUserId(), GetAuthHeader());
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // Card Labels 

    [HttpGet("cards/{cardId:guid}/labels")]
    [ProducesResponseType(typeof(IEnumerable<LabelResponse>), 200)]
    public async Task<IActionResult> GetLabelsForCard([FromRoute] Guid cardId)
    {
        try
        {
            var result = await _labelService.GetLabelsForCardAsync(cardId, GetCurrentUserId(), GetAuthHeader());
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpPost("cards/{cardId:guid}/labels/{labelId:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> AddLabelToCard([FromRoute] Guid cardId, [FromRoute] Guid labelId)
    {
        try
        {
            await _labelService.AddLabelToCardAsync(cardId, labelId, GetCurrentUserId(), GetAuthHeader());
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("cards/{cardId:guid}/labels/{labelId:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RemoveLabelFromCard([FromRoute] Guid cardId, [FromRoute] Guid labelId)
    {
        try
        {
            await _labelService.RemoveLabelFromCardAsync(cardId, labelId, GetCurrentUserId(), GetAuthHeader());
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // Checklists 

    [HttpPost("checklists")]
    [ProducesResponseType(typeof(ChecklistResponse), 201)]
    public async Task<IActionResult> CreateChecklist([FromBody] CreateChecklistRequest request)
    {
        try
        {
            var result = await _labelService.CreateChecklistAsync(GetCurrentUserId(), GetAuthHeader(), request);
            return StatusCode(201, result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("cards/{cardId:guid}/checklists")]
    [ProducesResponseType(typeof(IEnumerable<ChecklistResponse>), 200)]
    public async Task<IActionResult> GetChecklistsByCard([FromRoute] Guid cardId)
    {
        try
        {
            var result = await _labelService.GetChecklistsByCardAsync(cardId, GetCurrentUserId(), GetAuthHeader());
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpDelete("checklists/{checklistId:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteChecklist([FromRoute] Guid checklistId)
    {
        try
        {
            await _labelService.DeleteChecklistAsync(checklistId, GetCurrentUserId(), GetAuthHeader());
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // Checklist Items 

    [HttpPost("checklists/{checklistId:guid}/items")]
    [ProducesResponseType(typeof(ChecklistItemResponse), 201)]
    public async Task<IActionResult> AddItem([FromRoute] Guid checklistId, [FromBody] AddChecklistItemRequest request)
    {
        try
        {
            var result = await _labelService.AddItemAsync(checklistId, GetCurrentUserId(), GetAuthHeader(), request);
            return StatusCode(201, result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("items/{itemId:guid}/toggle")]
    [ProducesResponseType(typeof(ChecklistItemResponse), 200)]
    public async Task<IActionResult> ToggleItem([FromRoute] Guid itemId, [FromBody] ToggleItemRequest request)
    {
        try
        {
            var result = await _labelService.ToggleItemAsync(itemId, GetCurrentUserId(), GetAuthHeader(), request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("items/{itemId:guid}/assignee")]
    [ProducesResponseType(typeof(ChecklistItemResponse), 200)]
    public async Task<IActionResult> SetItemAssignee([FromRoute] Guid itemId, [FromBody] SetItemAssigneeRequest request)
    {
        try
        {
            var result = await _labelService.SetItemAssigneeAsync(itemId, GetCurrentUserId(), GetAuthHeader(), request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("items/{itemId:guid}/due-date")]
    [ProducesResponseType(typeof(ChecklistItemResponse), 200)]
    public async Task<IActionResult> SetItemDueDate([FromRoute] Guid itemId, [FromBody] SetItemDueDateRequest request)
    {
        try
        {
            var result = await _labelService.SetItemDueDateAsync(itemId, GetCurrentUserId(), GetAuthHeader(), request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("items/{itemId:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteItem([FromRoute] Guid itemId)
    {
        try
        {
            await _labelService.DeleteItemAsync(itemId, GetCurrentUserId(), GetAuthHeader());
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // Progress Computation 

    [HttpGet("cards/{cardId:guid}/progress")]
    [ProducesResponseType(typeof(ChecklistProgressResponse), 200)]
    public async Task<IActionResult> GetChecklistProgress([FromRoute] Guid cardId)
    {
        try
        {
            var result = await _labelService.GetChecklistProgressAsync(cardId, GetCurrentUserId(), GetAuthHeader());
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    // Private Helpers 

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")
            ?? throw new UnauthorizedAccessException("User identity not found in token.");
        return Guid.Parse(claim.Value);
    }

    private string GetAuthHeader()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authHeader))
            throw new UnauthorizedAccessException("Missing Authorization header.");
        return authHeader;
    }
}
