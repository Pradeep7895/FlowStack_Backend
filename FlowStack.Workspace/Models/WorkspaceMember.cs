using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.Workspace.Models;

[Table("workspace_members")]
public class WorkspaceMember
{
    [Key]
    public Guid MemberId { get; set; } = Guid.NewGuid();

    public Guid WorkspaceId { get; set; }

    public Guid UserId { get; set; }

    public WorkspaceMemberRole Role { get; set; } = WorkspaceMemberRole.Member;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(WorkspaceId))]
    public Workspace Workspace { get; set; } = null!;
}