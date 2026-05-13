using FlowStack.CommentService.Models;

namespace FlowStack.CommentService.Repositories;

/// Data access interface for Comment entities.
/// All methods returning active comments use the global query filter (IsDeleted = false).

public interface ICommentRepository
{
    Task<Comment?> FindByCommentIdAsync(Guid commentId);
    Task<IEnumerable<Comment>> FindByCardIdAsync(Guid cardId);
    Task<IEnumerable<Comment>> FindByAuthorIdAsync(Guid authorId);
    Task<IEnumerable<Comment>> FindByParentCommentIdAsync(Guid parentCommentId);

    Task<int> CountByCardIdAsync(Guid cardId);

    // Write operations
    Task<Comment> CreateAsync(Comment comment);
    Task<Comment> UpdateAsync(Comment comment);
    Task DeleteByCommentIdAsync(Guid commentId);

    // Attachments 
    Task<Attachment?> FindByAttachmentIdAsync(Guid attachmentId);
    Task<IEnumerable<Attachment>> FindAttachmentsByCardIdAsync(Guid cardId);
    Task<Attachment> CreateAsync(Attachment attachment);
    Task DeleteByAttachmentIdAsync(Guid attachmentId);
}
