using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.NotificationService.Models;

[Table("notifications")]
public class Notification
{
    [Key]
    public Guid NotificationId { get; set; } = Guid.NewGuid();

    // The user who receives the notification
    [Required]
    public Guid RecipientId { get; set; }

    // The user who triggered the action
    public Guid? ActorId { get; set; }

    // e.g. "ASSIGNMENT", "MENTION", "DUE_DATE", "COMMENT", "MOVE", "SYSTEM"
    [Required, MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    // The ID of the related entity (CardId, BoardId, etc)
    public Guid? RelatedId { get; set; }

    // The type of the related entity (e.g. "CARD", "BOARD")
    [MaxLength(50)]
    public string? RelatedType { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
