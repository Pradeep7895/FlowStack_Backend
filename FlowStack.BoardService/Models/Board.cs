using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.BoardService.Models;

public enum BoardVisibility
{
    Public,
    Private
}

public enum BoardMemberRole
{
    // Can view board but not edit
    Observer, 
    // Can create/edit cards and lists  
    Member, 
    // Can manage board settings and membership    
    Admin      
}

[Table("boards")]
public class Board
{
    [Key]
    public Guid BoardId { get; set; } = Guid.NewGuid();

    // References workspace-service — plain Guid, no cross-service FK
    public Guid WorkspaceId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Background { get; set; }

    public BoardVisibility Visibility { get; set; } = BoardVisibility.Private;

    // References auth-service — plain Guid, no cross-service FK
    public Guid CreatedById { get; set; }

    // When true board is read-only — no new cards/lists allowed
    public bool IsClosed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<BoardMember> Members { get; set; } = new List<BoardMember>();
}

