using System.ComponentModel.DataAnnotations;

namespace FlowStack.LabelService.DTOs;

public class CreateLabelRequest
{
    [Required]
    public Guid BoardId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(7)]
    public string Color { get; set; } = string.Empty;
}