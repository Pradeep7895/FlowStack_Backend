using System.ComponentModel.DataAnnotations;

namespace FlowStack.TaskService.DTOs;

public class UpdateCardRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(5000)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? StartDate { get; set; }

    [MaxLength(20)]
    public string? CoverColor { get; set; }
}