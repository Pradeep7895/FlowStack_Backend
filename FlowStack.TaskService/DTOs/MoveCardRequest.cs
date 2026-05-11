using System.ComponentModel.DataAnnotations;

namespace FlowStack.TaskService.DTOs;
public class MoveCardRequest
{
    [Required]
    public Guid TargetListId { get; set; }
    public int? TargetPosition { get; set; }
}
