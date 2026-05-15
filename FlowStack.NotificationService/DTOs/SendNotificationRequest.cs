using System.ComponentModel.DataAnnotations;

namespace FlowStack.NotificationService.DTOs;

public class SendNotificationRequest
{
    [Required]
    public Guid RecipientId { get; set; }
    
    public Guid? ActorId { get; set; }

    [Required, MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public Guid? RelatedId { get; set; }
    
    [MaxLength(50)]
    public string? RelatedType { get; set; }

    public bool SendEmail { get; set; } = false;
    public string? RecipientEmail { get; set; }
}