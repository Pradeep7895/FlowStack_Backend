using FlowStack.NotificationService.Models;

namespace FlowStack.NotificationService.Repositories;

public interface INotificationRepository
{
    Task<Notification?> FindByIdAsync(Guid notificationId);
    Task<IEnumerable<Notification>> FindByRecipientIdAsync(Guid recipientId);
    Task<IEnumerable<Notification>> FindByRecipientIdAndIsReadAsync(Guid recipientId, bool isRead);
    Task<int> CountByRecipientIdAndIsReadAsync(Guid recipientId, bool isRead);
    Task<IEnumerable<Notification>> FindByTypeAsync(string type);
    Task<IEnumerable<Notification>> FindByRelatedIdAsync(Guid relatedId);
    
    Task<Notification> CreateAsync(Notification notification);
    Task CreateBulkAsync(IEnumerable<Notification> notifications);
    
    Task<Notification> UpdateAsync(Notification notification);
    Task UpdateBulkAsync(IEnumerable<Notification> notifications);
    
    Task DeleteByNotificationIdAsync(Guid notificationId);
    Task DeleteByRecipientIdAndIsReadAsync(Guid recipientId, bool isRead);
    Task DeleteBulkAsync(IEnumerable<Notification> notifications);
}
