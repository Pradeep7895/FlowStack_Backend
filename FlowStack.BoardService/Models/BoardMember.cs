using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.BoardService.Models;

[Table("board_members")]
public class BoardMember
{
    [Key]
    public Guid BoardMemberId { get; set; } = Guid.NewGuid();

    public Guid BoardId { get; set; }

    // References auth-service — plain Guid
    public Guid UserId { get; set; }

    public BoardMemberRole Role { get; set; } = BoardMemberRole.Member;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(BoardId))]
    public Board Board { get; set; } = null!;
}