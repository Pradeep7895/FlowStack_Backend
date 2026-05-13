using FlowStack.NotificationService.DTOs;
using FlowStack.NotificationService.Models;
using FlowStack.NotificationService.Repositories;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FlowStack.NotificationService.Services;

public class NotificationServiceImpl : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly ILogger<NotificationServiceImpl> _logger;
    private readonly ISendGridClient _sendGridClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public NotificationServiceImpl(
        INotificationRepository repo,
        ILogger<NotificationServiceImpl> logger,
        IConfiguration config)
    {
        _repo = repo;
        _logger = logger;

        var apiKey = config["SendGrid:ApiKey"] ?? string.Empty;
        _sendGridClient = new SendGridClient(apiKey);
        _fromEmail = config["SendGrid:FromEmail"] ?? "noreply@flowstack.app";
        _fromName = config["SendGrid:FromName"] ?? "FlowStack";
    }

    public async Task<NotificationResponse> SendAsync(SendNotificationRequest request)
    {
        var notification = new Notification
        {
            RecipientId = request.RecipientId,
            ActorId = request.ActorId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            RelatedId = request.RelatedId,
            RelatedType = request.RelatedType
        };

        notification = await _repo.CreateAsync(notification);

        if (request.SendEmail && !string.IsNullOrWhiteSpace(request.RecipientEmail))
        {
            await SendEmailAsync(request.RecipientEmail, request.Title, request.Message);
        }

        return NotificationResponse.FromEntity(notification);
    }

    public async Task SendBulkAsync(SendBulkNotificationRequest request)
    {
        var notifications = request.RecipientIds.Select(recipientId => new Notification
        {
            RecipientId = recipientId,
            ActorId = request.ActorId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            RelatedId = request.RelatedId,
            RelatedType = request.RelatedType
        }).ToList();

        await _repo.CreateBulkAsync(notifications);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to send email to {Email}. Status: {StatusCode}", toEmail, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending email to {Email}", toEmail);
        }
    }

    public async Task<IEnumerable<NotificationResponse>> GetByRecipientAsync(Guid recipientId)
    {
        var notifications = await _repo.FindByRecipientIdAsync(recipientId);
        return notifications.Select(NotificationResponse.FromEntity);
    }

    public async Task<UnreadCountResponse> GetUnreadCountAsync(Guid recipientId)
    {
        var count = await _repo.CountByRecipientIdAndIsReadAsync(recipientId, false);
        return new UnreadCountResponse
        {
            RecipientId = recipientId,
            UnreadCount = count
        };
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid requesterId)
    {
        var notification = await _repo.FindByIdAsync(notificationId)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.RecipientId != requesterId)
            throw new UnauthorizedAccessException("You can only access your own notifications.");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _repo.UpdateAsync(notification);
        }
    }

    public async Task MarkAllReadAsync(Guid recipientId)
    {
        var unread = await _repo.FindByRecipientIdAndIsReadAsync(recipientId, false);
        var unreadList = unread.ToList();

        if (unreadList.Any())
        {
            foreach (var n in unreadList)
            {
                n.IsRead = true;
            }
            await _repo.UpdateBulkAsync(unreadList);
        }
    }

    public async Task DeleteNotificationAsync(Guid notificationId, Guid requesterId)
    {
        var notification = await _repo.FindByIdAsync(notificationId)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.RecipientId != requesterId)
            throw new UnauthorizedAccessException("You can only access your own notifications.");

        await _repo.DeleteByNotificationIdAsync(notificationId);
    }

    public async Task DeleteReadAsync(Guid recipientId)
    {
        await _repo.DeleteByRecipientIdAndIsReadAsync(recipientId, true);
    }

    public async Task<IEnumerable<NotificationResponse>> GetAllAsync()
    {
        throw new NotImplementedException("GetAll for admin not implemented.");
    }
}
