using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.CommentService.Models;

// Represents a comment on a card.
// Comments support two-level threading via ParentCommentId.

[Table("comments")]
public class Comment
{
    [Key]
    public Guid CommentId { get; set; } = Guid.NewGuid();

    // References task-service — which card this comment belongs to
    public Guid CardId { get; set; }

    // References auth-service — who wrote this comment
    public Guid AuthorId { get; set; }

    [Required, MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    public Guid? ParentCommentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
}
