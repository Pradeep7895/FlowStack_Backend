using FlowStack.Workspace.Models;

namespace FlowStack.Workspace.DTOs;

public class WorkspaceMemberResponse
{
    public Guid MemberId { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }

    public static WorkspaceMemberResponse FromMember(WorkspaceMember m) => new()
    {
        MemberId    = m.MemberId,
        WorkspaceId = m.WorkspaceId,
        UserId      = m.UserId,
        Role        = m.Role.ToString(),
        JoinedAt    = m.JoinedAt
    };
}