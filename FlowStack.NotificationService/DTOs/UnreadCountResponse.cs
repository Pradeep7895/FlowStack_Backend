
namespace FlowStack.NotificationService.DTOs;
public class UnreadCountResponse
{
    public Guid RecipientId { get; set; }
    public int UnreadCount { get; set; }
}