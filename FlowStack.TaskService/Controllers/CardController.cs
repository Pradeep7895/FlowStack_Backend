using FlowStack.TaskService.DTOs;
using FlowStack.TaskService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowStack.TaskService.Controllers;

[ApiController]
[Route("api/cards")]
[Authorize]
[Produces("application/json")]
public class CardController : ControllerBase
{
    private readonly ICardService _cardService;

    public CardController(ICardService cardService)
    {
        _cardService = cardService;
    }

    // Create 

    // POST /api/cards
    [HttpPost]
    [ProducesResponseType(typeof(CardResponse), 201)]
    [ProducesResponseType(400), ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] CreateCardRequest request)
    {
        try
        {
            var card = await _cardService.CreateCardAsync(GetCurrentUserId(), request);
            return CreatedAtAction(nameof(GetById), new { cardId = card.CardId }, card);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    //  Read 

    // GET /api/cards/{cardId}
    [HttpGet("{cardId:guid}")]
    [ProducesResponseType(typeof(CardResponse), 200)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> GetById([FromRoute] Guid cardId)
    {
        try
        {
            var card = await _cardService.GetCardByIdAsync(cardId, GetCurrentUserId());
            return Ok(card);
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

    // GET /api/cards/list/{listId}
    [HttpGet("list/{listId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<CardResponse>), 200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetByList([FromRoute] Guid listId)
    {
        try
        {
            var cards = await _cardService.GetCardsByListAsync(listId, GetCurrentUserId());
            return Ok(cards);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // GET /api/cards/board/{boardId}
    [HttpGet("board/{boardId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<CardResponse>), 200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetByBoard([FromRoute] Guid boardId)
    {
        try
        {
            var cards = await _cardService.GetCardsByBoardAsync(boardId, GetCurrentUserId());
            return Ok(cards);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // GET /api/cards/assignee/{assigneeId}
    [HttpGet("assignee/{assigneeId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<CardResponse>), 200)]
    public async Task<IActionResult> GetByAssignee([FromRoute] Guid assigneeId)
    {
        var cards = await _cardService.GetCardsByAssigneeAsync(assigneeId);
        return Ok(cards);
    }

    // GET /api/cards/overdue
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IEnumerable<CardResponse>), 200)]
    public async Task<IActionResult> GetOverdue()
    {
        var cards = await _cardService.GetOverdueCardsAsync(GetCurrentUserId());
        return Ok(cards);
    }

    // Update 

    // PUT /api/cards/{cardId}
    [HttpPut("{cardId:guid}")]
    [ProducesResponseType(typeof(CardResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid cardId,
        [FromBody]  UpdateCardRequest request)
    {
        try
        {
            var card = await _cardService.UpdateCardAsync(cardId, GetCurrentUserId(), request);
            return Ok(card);
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/cards/{cardId}/move
    [HttpPut("{cardId:guid}/move")]
    [ProducesResponseType(typeof(CardResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Move(
        [FromRoute] Guid cardId,
        [FromBody]  MoveCardRequest request)
    {
        try
        {
            var card = await _cardService.MoveCardAsync(cardId, GetCurrentUserId(), request);
            return Ok(card);
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

    // PUT /api/cards/reorder
    [HttpPut("reorder")]
    [ProducesResponseType(typeof(IEnumerable<CardResponse>), 200)]
    [ProducesResponseType(400), ProducesResponseType(403)]
    public async Task<IActionResult> Reorder([FromBody] ReorderCardsRequest request)
    {
        try
        {
            var cards = await _cardService.ReorderCardsAsync(GetCurrentUserId(), request);
            return Ok(cards);
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

    // PUT /api/cards/{cardId}/assignee
    [HttpPut("{cardId:guid}/assignee")]
    [ProducesResponseType(typeof(CardResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> SetAssignee(
        [FromRoute] Guid cardId,
        [FromBody]  SetAssigneeRequest request)
    {
        try
        {
            var card = await _cardService.SetAssigneeAsync(cardId, GetCurrentUserId(), request);
            return Ok(card);
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

    // PUT /api/cards/{cardId}/priority
    [HttpPut("{cardId:guid}/priority")]
    [ProducesResponseType(typeof(CardResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> SetPriority(
        [FromRoute] Guid cardId,
        [FromBody]  SetPriorityRequest request)
    {
        try
        {
            var card = await _cardService.SetPriorityAsync(cardId, GetCurrentUserId(), request);
            return Ok(card);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Archive / Unarchive 

    // POST /api/cards/{cardId}/archive
    [HttpPost("{cardId:guid}/archive")]
    [ProducesResponseType(typeof(CardResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Archive([FromRoute] Guid cardId)
    {
        try
        {
            var card = await _cardService.ArchiveCardAsync(cardId, GetCurrentUserId());
            return Ok(card);
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

    // POST /api/cards/{cardId}/unarchive
    [HttpPost("{cardId:guid}/unarchive")]
    [ProducesResponseType(typeof(CardResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Unarchive([FromRoute] Guid cardId)
    {
        try
        {
            var card = await _cardService.UnarchiveCardAsync(cardId, GetCurrentUserId());
            return Ok(card);
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

    // Delete 

    // DELETE /api/cards/{cardId}
    [HttpDelete("{cardId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Delete([FromRoute] Guid cardId)
    {
        try
        {
            await _cardService.DeleteCardAsync(cardId, GetCurrentUserId());
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
