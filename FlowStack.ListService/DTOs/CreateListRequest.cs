using System.ComponentModel.DataAnnotations;

namespace FlowStack.ListService.DTOs;

public class CreateListRequest
{
    [Required]
    public Guid BoardId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Color { get; set; }
}

