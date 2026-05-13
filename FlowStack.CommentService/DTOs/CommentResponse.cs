namespace FlowStack.CommentService.DTOs;

public class CommentResponse
{
    public Guid CommentId { get; set; }
    public Guid CardId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static CommentResponse FromComment(Models.Comment comment) => new()
    {
        CommentId = comment.CommentId,
        CardId = comment.CardId,
        AuthorId = comment.AuthorId,
        Content = comment.Content,
        ParentCommentId = comment.ParentCommentId,
        CreatedAt = comment.CreatedAt,
        UpdatedAt = comment.UpdatedAt
    };
}