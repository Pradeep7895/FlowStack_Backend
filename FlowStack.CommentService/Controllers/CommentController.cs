using FlowStack.CommentService.DTOs;
using FlowStack.CommentService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowStack.CommentService.Controllers;

[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    // Comments 

    // POST /api/comments
    [HttpPost("comments")]
    [ProducesResponseType(typeof(CommentResponse), 201)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
    {
        try
        {
            var comment = await _commentService.AddCommentAsync(
                GetCurrentUserId(), GetAuthHeader(), request);
            return StatusCode(201, comment);
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

    // GET /api/cards/{cardId}/comments
    [HttpGet("cards/{cardId:guid}/comments")]
    [ProducesResponseType(typeof(IEnumerable<CommentResponse>), 200)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> GetCardComments([FromRoute] Guid cardId)
    {
        try
        {
            var comments = await _commentService.GetCardCommentsAsync(
                cardId, GetCurrentUserId(), GetAuthHeader());
            return Ok(comments);
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

    // GET /api/comments/{commentId}/replies
    [HttpGet("comments/{commentId:guid}/replies")]
    [ProducesResponseType(typeof(IEnumerable<CommentResponse>), 200)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> GetReplies([FromRoute] Guid commentId)
    {
        try
        {
            var replies = await _commentService.GetRepliesAsync(
                commentId, GetCurrentUserId(), GetAuthHeader());
            return Ok(replies);
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

    // PUT /api/comments/{commentId}
    [HttpPut("comments/{commentId:guid}")]
    [ProducesResponseType(typeof(CommentResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> UpdateComment(
        [FromRoute] Guid commentId,
        [FromBody]  UpdateCommentRequest request)
    {
        try
        {
            var comment = await _commentService.UpdateCommentAsync(
                commentId, GetCurrentUserId(), GetAuthHeader(), request);
            return Ok(comment);
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

    // DELETE /api/comments/{commentId}
    [HttpDelete("comments/{commentId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> DeleteComment([FromRoute] Guid commentId)
    {
        try
        {
            await _commentService.DeleteCommentAsync(
                commentId, GetCurrentUserId(), GetAuthHeader());
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

    // GET /api/cards/{cardId}/comments/count
    [HttpGet("cards/{cardId:guid}/comments/count")]
    [ProducesResponseType(typeof(CommentCountResponse), 200)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> GetCommentCount([FromRoute] Guid cardId)
    {
        try
        {
            var count = await _commentService.GetCommentCountAsync(
                cardId, GetCurrentUserId(), GetAuthHeader());
            return Ok(count);
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

    // Attachments 

    // POST /api/attachments
    [HttpPost("attachments")]
    [ProducesResponseType(typeof(AttachmentResponse), 201)]
    [ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> AddAttachment([FromBody] AddAttachmentRequest request)
    {
        try
        {
            var attachment = await _commentService.AddAttachmentAsync(GetCurrentUserId(), GetAuthHeader(), request);
            return StatusCode(201, attachment);
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

    // GET /api/cards/{cardId}/attachments
    [HttpGet("cards/{cardId:guid}/attachments")]
    [ProducesResponseType(typeof(IEnumerable<AttachmentResponse>), 200)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> GetCardAttachments([FromRoute] Guid cardId)
    {
        try
        {
            var attachments = await _commentService.GetCardAttachmentsAsync(
                cardId, GetCurrentUserId(), GetAuthHeader());
            return Ok(attachments);
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

    // DELETE /api/attachments/{attachmentId}
    [HttpDelete("attachments/{attachmentId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAttachment([FromRoute] Guid attachmentId)
    {
        try
        {
            await _commentService.DeleteAttachmentAsync(attachmentId, GetCurrentUserId(), GetAuthHeader());
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

    private string GetAuthHeader()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authHeader))
            throw new UnauthorizedAccessException("Missing Authorization header.");
        return authHeader;
    }
}
