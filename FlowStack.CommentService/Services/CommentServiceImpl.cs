using FlowStack.CommentService.DTOs;
using FlowStack.CommentService.Helpers;
using FlowStack.CommentService.Models;
using FlowStack.CommentService.Repositories;

namespace FlowStack.CommentService.Services;

public class CommentServiceImpl : ICommentService
{
    private readonly ICommentRepository _commentRepo;
    private readonly TaskClient _taskClient;
    private readonly BoardClient _boardClient;

    public CommentServiceImpl(
        ICommentRepository commentRepo,
        TaskClient taskClient,
        BoardClient boardClient)
    {
        _commentRepo = commentRepo;
        _taskClient = taskClient;
        _boardClient = boardClient;
    }

    // Comments 

    public async Task<CommentResponse> AddCommentAsync(
        Guid requesterId, string authHeader, AddCommentRequest request)
    {
        var boardId = await _taskClient.GetBoardIdForCardAsync(request.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await _boardClient.GetBoardAccessAsync(boardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to comment.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot add comments to a closed board.");

        // Observers are typically allowed to comment in many systems,
        // but if the spec requires otherwise, we can restrict it. We'll allow observers to comment here,
        // but they can't move/edit cards. If strictly no-write, uncomment below.

        if (request.ParentCommentId.HasValue)
        {
            var parent = await _commentRepo.FindByCommentIdAsync(request.ParentCommentId.Value)
                ?? throw new KeyNotFoundException("Parent comment not found.");

            if (parent.CardId != request.CardId)
                throw new InvalidOperationException("Parent comment belongs to a different card.");

            // Limit to 2 levels of threading: parent cannot already have a parent
            if (parent.ParentCommentId.HasValue)
                throw new InvalidOperationException("Only one level of replies is supported.");
        }

        var comment = new Comment
        {
            CardId = request.CardId,
            AuthorId = requesterId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId
        };

        comment = await _commentRepo.CreateAsync(comment);

        return CommentResponse.FromComment(comment);
    }

    public async Task<CommentResponse> GetCommentByIdAsync(
        Guid commentId, Guid requesterId, string authHeader)
    {
        var comment = await _commentRepo.FindByCommentIdAsync(commentId)
            ?? throw new KeyNotFoundException($"Comment {commentId} not found.");

        // Implicitly checks if the user has access to the card
        var boardId = await _taskClient.GetBoardIdForCardAsync(comment.CardId, authHeader)
            ?? throw new UnauthorizedAccessException("Access denied.");

        return CommentResponse.FromComment(comment);
    }

    public async Task<IEnumerable<CommentResponse>> GetCardCommentsAsync(
        Guid cardId, Guid requesterId, string authHeader)
    {
        var boardId = await _taskClient.GetBoardIdForCardAsync(cardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var comments = await _commentRepo.FindByCardIdAsync(cardId);
        return comments.Select(CommentResponse.FromComment);
    }

    public async Task<IEnumerable<CommentResponse>> GetRepliesAsync(
        Guid parentCommentId, Guid requesterId, string authHeader)
    {
        var parent = await _commentRepo.FindByCommentIdAsync(parentCommentId)
            ?? throw new KeyNotFoundException("Parent comment not found.");

        var boardId = await _taskClient.GetBoardIdForCardAsync(parent.CardId, authHeader)
            ?? throw new UnauthorizedAccessException("Access denied.");

        var replies = await _commentRepo.FindByParentCommentIdAsync(parentCommentId);
        return replies.Select(CommentResponse.FromComment);
    }

    public async Task<CommentResponse> UpdateCommentAsync(
        Guid commentId, Guid requesterId, string authHeader, UpdateCommentRequest request)
    {
        var comment = await _commentRepo.FindByCommentIdAsync(commentId)
            ?? throw new KeyNotFoundException("Comment not found.");

        if (comment.AuthorId != requesterId)
            throw new UnauthorizedAccessException("You can only edit your own comments.");

        var boardId = await _taskClient.GetBoardIdForCardAsync(comment.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await _boardClient.GetBoardAccessAsync(boardId, requesterId);
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot edit comments on a closed board.");

        comment.Content = request.Content;
        comment = await _commentRepo.UpdateAsync(comment);

        return CommentResponse.FromComment(comment);
    }

    public async Task DeleteCommentAsync(
        Guid commentId, Guid requesterId, string authHeader)
    {
        var comment = await _commentRepo.FindByCommentIdAsync(commentId)
            ?? throw new KeyNotFoundException("Comment not found.");

        var boardId = await _taskClient.GetBoardIdForCardAsync(comment.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await _boardClient.GetBoardAccessAsync(boardId, requesterId);

        // Admins can delete any comment; users can only delete their own
        if (comment.AuthorId != requesterId && !access.IsAdminOrCreator)
            throw new UnauthorizedAccessException("You don't have permission to delete this comment.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot delete comments on a closed board.");

        await _commentRepo.DeleteByCommentIdAsync(commentId);
    }

    public async Task<CommentCountResponse> GetCommentCountAsync(
        Guid cardId, Guid requesterId, string authHeader)
    {
        var boardId = await _taskClient.GetBoardIdForCardAsync(cardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var count = await _commentRepo.CountByCardIdAsync(cardId);
        return new CommentCountResponse { CardId = cardId, Count = count };
    }

    //  Attachments 

    public async Task<AttachmentResponse> AddAttachmentAsync(
        Guid requesterId, string authHeader, AddAttachmentRequest request)
    {
        var boardId = await _taskClient.GetBoardIdForCardAsync(request.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await _boardClient.GetBoardAccessAsync(boardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to add attachments.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot add attachments to a closed board.");

        var attachment = new Attachment
        {
            CardId = request.CardId,
            UploaderId = requesterId,
            FileName = request.FileName,
            FileUrl = request.FileUrl,
            FileType = request.FileType,
            SizeKb = request.SizeKb
        };

        attachment = await _commentRepo.CreateAsync(attachment);

        return AttachmentResponse.FromAttachment(attachment);
    }

    public async Task<IEnumerable<AttachmentResponse>> GetCardAttachmentsAsync(
        Guid cardId, Guid requesterId, string authHeader)
    {
        var boardId = await _taskClient.GetBoardIdForCardAsync(cardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var attachments = await _commentRepo.FindAttachmentsByCardIdAsync(cardId);
        return attachments.Select(AttachmentResponse.FromAttachment);
    }

    public async Task DeleteAttachmentAsync(
        Guid attachmentId, Guid requesterId, string authHeader)
    {
        var attachment = await _commentRepo.FindByAttachmentIdAsync(attachmentId)
            ?? throw new KeyNotFoundException("Attachment not found.");

        var boardId = await _taskClient.GetBoardIdForCardAsync(attachment.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await _boardClient.GetBoardAccessAsync(boardId, requesterId);

        // Admins can delete any attachment; users can only delete their own
        if (attachment.UploaderId != requesterId && !access.IsAdminOrCreator)
            throw new UnauthorizedAccessException("You don't have permission to delete this attachment.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot delete attachments on a closed board.");

        await _commentRepo.DeleteByAttachmentIdAsync(attachmentId);
    }
}
