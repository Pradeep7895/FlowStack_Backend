using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.LabelService.Models;

[Table("checklists")]
public class Checklist
{
    [Key]
    public Guid ChecklistId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CardId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int Position { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public List<ChecklistItem> Items { get; set; } = new();
}

