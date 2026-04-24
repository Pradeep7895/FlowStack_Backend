namespace FlowStack.Workspace.DTOs;
public class WorkspaceDetailResponse : WorkspaceResponse
{
    public IEnumerable<WorkspaceMemberResponse> Members { get; set; } = Enumerable.Empty<WorkspaceMemberResponse>();

    public static WorkspaceDetailResponse FromWorkspaceWithMembers(Models.Workspace w) => new()
    {
        WorkspaceId  = w.WorkspaceId,
        Name         = w.Name,
        Description  = w.Description,
        OwnerId      = w.OwnerId,
        Visibility   = w.Visibility.ToString(),
        LogoUrl      = w.LogoUrl,
        MemberCount  = w.Members?.Count ?? 0,
        CreatedAt    = w.CreatedAt,
        UpdatedAt    = w.UpdatedAt,
        Members      = w.Members?.Select(WorkspaceMemberResponse.FromMember)
                        ?? Enumerable.Empty<WorkspaceMemberResponse>()
    };
}