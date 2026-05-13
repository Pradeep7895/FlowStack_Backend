using System.ComponentModel.DataAnnotations;

namespace FlowStack.CommentService.DTOs;

public class UpdateCommentRequest
{
    [Required, MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}