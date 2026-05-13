using System.ComponentModel.DataAnnotations;

namespace FlowStack.LabelService.DTOs;

public class AddChecklistItemRequest
{
    [Required, MaxLength(500)]
    public string Text { get; set; } = string.Empty;
}