using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.ListService.Models;

[Table("task_lists")]
public class TaskList
{
    [Key]
    public Guid ListId { get; set; } = Guid.NewGuid();

    // References board-service — plain Guid, no cross-service FK
    public Guid BoardId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Left-to-right display order. Starts at 1. Gaps allowed.
    public int Position { get; set; }

    // Optional hex colour code e.g. "#FF5733"
    [MaxLength(20)]
    public string? Color { get; set; }

    // Soft-delete via EF Core query filter — hidden from normal queries
    public bool IsArchived { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}