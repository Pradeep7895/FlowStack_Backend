namespace FlowStack.CommentService.DTOs;

public class AttachmentResponse
{
    public Guid AttachmentId { get; set; }
    public Guid CardId { get; set; }
    public Guid UploaderId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long SizeKb { get; set; }
    public DateTime UploadedAt { get; set; }

    public static AttachmentResponse FromAttachment(Models.Attachment attachment) => new()
    {
        AttachmentId = attachment.AttachmentId,
        CardId = attachment.CardId,
        UploaderId = attachment.UploaderId,
        FileName = attachment.FileName,
        FileUrl = attachment.FileUrl,
        FileType = attachment.FileType,
        SizeKb = attachment.SizeKb,
        UploadedAt = attachment.UploadedAt
    };
}