
namespace FlowStack.Workspace.DTOs;

public class WorkspaceResponse
{
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static WorkspaceResponse FromWorkspace(Models.Workspace w) => new()
    {
        WorkspaceId  = w.WorkspaceId,
        Name         = w.Name,
        Description  = w.Description,
        OwnerId      = w.OwnerId,
        Visibility   = w.Visibility.ToString(),
        LogoUrl      = w.LogoUrl,
        MemberCount  = w.Members?.Count ?? 0,
        CreatedAt    = w.CreatedAt,
        UpdatedAt    = w.UpdatedAt
    };
}