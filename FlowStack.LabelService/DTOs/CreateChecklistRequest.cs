using System.ComponentModel.DataAnnotations;

namespace FlowStack.LabelService.DTOs;
public class CreateChecklistRequest
{
    [Required]
    public Guid CardId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}