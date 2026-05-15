using System.ComponentModel.DataAnnotations;

namespace FlowStack.CommentService.DTOs;

public class AddCommentRequest
{
    [Required]
    public Guid CardId { get; set; }

    [Required, MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    public Guid? ParentCommentId { get; set; }
}
