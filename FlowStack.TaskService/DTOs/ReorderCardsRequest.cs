using System.ComponentModel.DataAnnotations;

namespace FlowStack.TaskService.DTOs;

public class ReorderCardsRequest
{
    [Required]
    public Guid ListId { get; set; }

    /// <summary>
    /// Complete ordered list of card IDs representing the new order.
    /// Must contain all active card IDs for the list — no extras, no missing.
    /// </summary>
    [Required]
    public List<Guid> OrderedCardIds { get; set; } = new();
}
