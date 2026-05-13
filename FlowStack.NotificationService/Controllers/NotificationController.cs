using FlowStack.NotificationService.DTOs;
using FlowStack.NotificationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowStack.NotificationService.Controllers;

[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notifService;

    public NotificationController(INotificationService notifService)
    {
        _notifService = notifService;
    }

    [HttpGet("notifications")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), 200)]
    public async Task<IActionResult> GetMyNotifications()
    {
        var result = await _notifService.GetByRecipientAsync(GetCurrentUserId());
        return Ok(result);
    }

    [HttpGet("notifications/unread-count")]
    [ProducesResponseType(typeof(UnreadCountResponse), 200)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _notifService.GetUnreadCountAsync(GetCurrentUserId());
        return Ok(result);
    }

    [HttpPut("notifications/{notificationId:guid}/read")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAsRead([FromRoute] Guid notificationId)
    {
        try
        {
            await _notifService.MarkAsReadAsync(notificationId, GetCurrentUserId());
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpPut("notifications/read-all")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notifService.MarkAllReadAsync(GetCurrentUserId());
        return NoContent();
    }

    [HttpDelete("notifications/{notificationId:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete([FromRoute] Guid notificationId)
    {
        try
        {
            await _notifService.DeleteNotificationAsync(notificationId, GetCurrentUserId());
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpDelete("notifications/read")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteRead()
    {
        await _notifService.DeleteReadAsync(GetCurrentUserId());
        return NoContent();
    }

    // This is for internal service-to-service calls (e.g. from TaskService or CommentService)
    // In production, this should be secured via an API Key or Client Credentials grant, 
    // but for now we'll allow an admin or internal token to call it.
    [HttpPost("notifications/bulk")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SendBulk([FromBody] SendBulkNotificationRequest request)
    {
        await _notifService.SendBulkAsync(request);
        return NoContent();
    }

    // Private Helpers 

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")
            ?? throw new UnauthorizedAccessException("User identity not found in token.");
        return Guid.Parse(claim.Value);
    }
}
