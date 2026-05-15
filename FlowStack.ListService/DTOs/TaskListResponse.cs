
namespace FlowStack.ListService.DTOs;
public class TaskListResponse
{
    public Guid ListId { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Position { get; set; }
    public string? Color { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static TaskListResponse FromTaskList(Models.TaskList list) => new()
    {
        ListId     = list.ListId,
        BoardId    = list.BoardId,
        Name       = list.Name,
        Position   = list.Position,
        Color      = list.Color,
        IsArchived = list.IsArchived,
        CreatedAt  = list.CreatedAt,
        UpdatedAt  = list.UpdatedAt
    };
}