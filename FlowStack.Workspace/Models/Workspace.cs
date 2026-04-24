using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.Workspace.Models;

public enum WorkspaceVisibility
{
    Public,
    Private
}

public enum WorkspaceMemberRole
{
    Admin,
    Member
}

[Table("workspaces")]
public class Workspace
{
    [Key]
    public Guid WorkspaceId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid OwnerId { get; set; }

    public WorkspaceVisibility Visibility { get; set; } = WorkspaceVisibility.Private;

    public string? LogoUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation 
    public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
}

