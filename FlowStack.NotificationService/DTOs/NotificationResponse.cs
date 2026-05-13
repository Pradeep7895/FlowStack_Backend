
namespace FlowStack.NotificationService.DTOs;

public class NotificationResponse
{
    public Guid NotificationId { get; set; }
    public Guid RecipientId { get; set; }
    public Guid? ActorId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedId { get; set; }
    public string? RelatedType { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public static NotificationResponse FromEntity(Models.Notification notification) => new()
    {
        NotificationId = notification.NotificationId,
        RecipientId    = notification.RecipientId,
        ActorId        = notification.ActorId,
        Type           = notification.Type,
        Title          = notification.Title,
        Message        = notification.Message,
        RelatedId      = notification.RelatedId,
        RelatedType    = notification.RelatedType,
        IsRead         = notification.IsRead,
        CreatedAt      = notification.CreatedAt
    };
}