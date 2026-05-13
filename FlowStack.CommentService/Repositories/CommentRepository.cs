using FlowStack.CommentService.Data;
using FlowStack.CommentService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.CommentService.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly CommentDbContext _db;

    public CommentRepository(CommentDbContext db)
    {
        _db = db;
    }


    public async Task<Comment?> FindByCommentIdAsync(Guid commentId) =>
        await _db.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

    // Card-scoped queries — top-level comments only, ordered chronologically

    public async Task<IEnumerable<Comment>> FindByCardIdAsync(Guid cardId) =>
        await _db.Comments
            .Where(c => c.CardId == cardId && c.ParentCommentId == null)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

    // Author queries

    public async Task<IEnumerable<Comment>> FindByAuthorIdAsync(Guid authorId) =>
        await _db.Comments
            .Where(c => c.AuthorId == authorId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    // Thread queries — find replies to a parent comment

    public async Task<IEnumerable<Comment>> FindByParentCommentIdAsync(Guid parentCommentId) =>
        await _db.Comments
            .Where(c => c.ParentCommentId == parentCommentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();


    public async Task<int> CountByCardIdAsync(Guid cardId) =>
        await _db.Comments.CountAsync(c => c.CardId == cardId);

    // Write operations

    public async Task<Comment> CreateAsync(Comment comment)
    {
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment> UpdateAsync(Comment comment)
    {
        comment.UpdatedAt = DateTime.UtcNow;
        _db.Comments.Update(comment);
        await _db.SaveChangesAsync();
        return comment;
    }

    public async Task DeleteByCommentIdAsync(Guid commentId)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment is not null)
        {
            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    //  Attachments 

    public async Task<Attachment?> FindByAttachmentIdAsync(Guid attachmentId) =>
        await _db.Attachments.FirstOrDefaultAsync(a => a.AttachmentId == attachmentId);

    public async Task<IEnumerable<Attachment>> FindAttachmentsByCardIdAsync(Guid cardId) =>
        await _db.Attachments
            .Where(a => a.CardId == cardId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync();

    public async Task<Attachment> CreateAsync(Attachment attachment)
    {
        _db.Attachments.Add(attachment);
        await _db.SaveChangesAsync();
        return attachment;
    }

    public async Task DeleteByAttachmentIdAsync(Guid attachmentId)
    {
        var attachment = await _db.Attachments
            .FirstOrDefaultAsync(a => a.AttachmentId == attachmentId);
        if (attachment is not null)
        {
            _db.Attachments.Remove(attachment);
            await _db.SaveChangesAsync();
        }
    }
}
