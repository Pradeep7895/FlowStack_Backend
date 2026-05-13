using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.LabelService.Models;

[Table("card_labels")]
public class CardLabel
{
    [Required]
    public Guid CardId { get; set; }

    [Required]
    public Guid LabelId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Label? Label { get; set; }
}
