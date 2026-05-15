using System.ComponentModel.DataAnnotations;

namespace FlowStack.TaskService.DTOs;

public class SetPriorityRequest
{
    [Required, MaxLength(20)]
    public string Priority { get; set; } = "MEDIUM";
}