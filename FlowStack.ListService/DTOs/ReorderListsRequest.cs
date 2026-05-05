using System.ComponentModel.DataAnnotations;

namespace FlowStack.ListService.DTOs;

public class ReorderListsRequest
{
    [Required]
    public Guid BoardId { get; set; }

    // ListIds in the new desired order (left → right).
    [Required]
    public List<Guid> OrderedListIds { get; set; } = new();
}