using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.LabelService.Models;

[Table("checklist_items")]
public class ChecklistItem
{
    [Key]
    public Guid ItemId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ChecklistId { get; set; }

    [Required, MaxLength(500)]
    public string Text { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    public Guid? AssigneeId { get; set; }

    public DateOnly? DueDate { get; set; }

    public int Position { get; set; }

    // Navigation property
    public Checklist? Checklist { get; set; }
}
