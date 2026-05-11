using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.TaskService.Models;

[Table("cards")]
public class Card
{
    [Key]
    public Guid CardId { get; set; } = Guid.NewGuid();

    // References list-service
    public Guid ListId { get; set; }

    // References board-service
    public Guid BoardId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Description { get; set; }

    // Left-to-right / top-to-bottom
    public int Position { get; set; }

    // LOW, MEDIUM, HIGH, CRITICAL
    [MaxLength(20)]
    public string Priority { get; set; } = "MEDIUM";

    // Status: TO_DO, IN_PROGRESS, IN_REVIEW, DONE
    [MaxLength(20)]
    public string Status { get; set; } = "TO_DO";

    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }

    // References auth-service — who is assigned to this card
    public Guid? AssigneeId { get; set; }

    public Guid CreatedById { get; set; }

    public bool IsArchived { get; set; } = false;

    [MaxLength(20)]
    public string? CoverColor { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
