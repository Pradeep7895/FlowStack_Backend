using FlowStack.NotificationService.DTOs;

namespace FlowStack.NotificationService.Services;

public interface INotificationService
{
    Task<NotificationResponse> SendAsync(SendNotificationRequest request);
    Task SendBulkAsync(SendBulkNotificationRequest request);
    Task SendEmailAsync(string toEmail, string subject, string body);

    Task<IEnumerable<NotificationResponse>> GetByRecipientAsync(Guid recipientId);
    Task<UnreadCountResponse> GetUnreadCountAsync(Guid recipientId);
    
    Task MarkAsReadAsync(Guid notificationId, Guid requesterId);
    Task MarkAllReadAsync(Guid recipientId);
    
    Task DeleteNotificationAsync(Guid notificationId, Guid requesterId);
    Task DeleteReadAsync(Guid recipientId);
    
    // For admin/system
    Task<IEnumerable<NotificationResponse>> GetAllAsync();
}
