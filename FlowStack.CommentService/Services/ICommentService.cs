using FlowStack.CommentService.DTOs;

namespace FlowStack.CommentService.Services;

public interface ICommentService
{
    // Comments
    Task<CommentResponse> AddCommentAsync(Guid requesterId, string authHeader, AddCommentRequest request);
    Task<CommentResponse> GetCommentByIdAsync(Guid commentId, Guid requesterId, string authHeader);
    Task<IEnumerable<CommentResponse>> GetCardCommentsAsync(Guid cardId, Guid requesterId, string authHeader);
    Task<IEnumerable<CommentResponse>> GetRepliesAsync(Guid parentCommentId, Guid requesterId, string authHeader);
    Task<CommentResponse> UpdateCommentAsync(Guid commentId, Guid requesterId, string authHeader, UpdateCommentRequest request);
    Task DeleteCommentAsync(Guid commentId, Guid requesterId, string authHeader);
    Task<CommentCountResponse> GetCommentCountAsync(Guid cardId, Guid requesterId, string authHeader);

    // Attachments
    Task<AttachmentResponse> AddAttachmentAsync(Guid requesterId, string authHeader, AddAttachmentRequest request);
    Task<IEnumerable<AttachmentResponse>> GetCardAttachmentsAsync(Guid cardId, Guid requesterId, string authHeader);
    Task DeleteAttachmentAsync(Guid attachmentId, Guid requesterId, string authHeader);
}
