using System.ComponentModel.DataAnnotations;

namespace FlowStack.TaskService.DTOs;

public class CreateCardRequest
{
    [Required]
    public Guid ListId { get; set; }

    [Required]
    public Guid BoardId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    [MaxLength(20)]
    public string? CoverColor { get; set; }
}


