using FlowStack.NotificationService.Data;
using FlowStack.NotificationService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.NotificationService.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _db;

    public NotificationRepository(NotificationDbContext db)
    {
        _db = db;
    }

    public async Task<Notification?> FindByIdAsync(Guid notificationId) =>
        await _db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);

    public async Task<IEnumerable<Notification>> FindByRecipientIdAsync(Guid recipientId) =>
        await _db.Notifications
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Notification>> FindByRecipientIdAndIsReadAsync(Guid recipientId, bool isRead) =>
        await _db.Notifications
            .Where(n => n.RecipientId == recipientId && n.IsRead == isRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<int> CountByRecipientIdAndIsReadAsync(Guid recipientId, bool isRead) =>
        await _db.Notifications.CountAsync(n => n.RecipientId == recipientId && n.IsRead == isRead);

    public async Task<IEnumerable<Notification>> FindByTypeAsync(string type) =>
        await _db.Notifications
            .Where(n => n.Type == type)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Notification>> FindByRelatedIdAsync(Guid relatedId) =>
        await _db.Notifications
            .Where(n => n.RelatedId == relatedId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
        return notification;
    }

    public async Task CreateBulkAsync(IEnumerable<Notification> notifications)
    {
        _db.Notifications.AddRange(notifications);
        await _db.SaveChangesAsync();
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        _db.Notifications.Update(notification);
        await _db.SaveChangesAsync();
        return notification;
    }

    public async Task UpdateBulkAsync(IEnumerable<Notification> notifications)
    {
        _db.Notifications.UpdateRange(notifications);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteByNotificationIdAsync(Guid notificationId)
    {
        var notification = await FindByIdAsync(notificationId);
        if (notification is not null)
        {
            _db.Notifications.Remove(notification);
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteByRecipientIdAndIsReadAsync(Guid recipientId, bool isRead)
    {
        var notifications = await FindByRecipientIdAndIsReadAsync(recipientId, isRead);
        if (notifications.Any())
        {
            _db.Notifications.RemoveRange(notifications);
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteBulkAsync(IEnumerable<Notification> notifications)
    {
        _db.Notifications.RemoveRange(notifications);
        await _db.SaveChangesAsync();
    }
}
