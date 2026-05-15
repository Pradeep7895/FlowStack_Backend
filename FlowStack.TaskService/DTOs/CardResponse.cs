namespace FlowStack.TaskService.DTOs;

public class CardResponse
{
    public Guid CardId { get; set; }
    public Guid ListId { get; set; }
    public Guid BoardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Position { get; set; }
    public string Priority { get; set; } = "MEDIUM";
    public string Status { get; set; } = "TO_DO";
    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid CreatedById { get; set; }
    public bool IsArchived { get; set; }
    public string? CoverColor { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static CardResponse FromCard(Models.Card card) => new()
    {
        CardId      = card.CardId,
        ListId      = card.ListId,
        BoardId     = card.BoardId,
        Title       = card.Title,
        Description = card.Description,
        Position    = card.Position,
        Priority    = card.Priority,
        Status      = card.Status,
        DueDate     = card.DueDate,
        StartDate   = card.StartDate,
        AssigneeId  = card.AssigneeId,
        CreatedById = card.CreatedById,
        IsArchived  = card.IsArchived,
        CoverColor  = card.CoverColor,
        CreatedAt   = card.CreatedAt,
        UpdatedAt   = card.UpdatedAt
    };
}
