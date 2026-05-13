using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.LabelService.Models;

[Table("labels")]
public class Label
{
    [Key]
    public Guid LabelId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid BoardId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Hex color code, e.g. "#FF5733"
    [Required, MaxLength(7)]
    public string Color { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
