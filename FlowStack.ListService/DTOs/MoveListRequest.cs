using System.ComponentModel.DataAnnotations;

namespace FlowStack.ListService.DTOs;

public class MoveListRequest
{
    // Position is appended at the end of the target board's list.
    [Required]
    public Guid TargetBoardId { get; set; }
}