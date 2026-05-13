using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowStack.CommentService.Models;

// Represents a file attachment linked to a card.
// Files are stored in Cloudinary.
[Table("attachments")]
public class Attachment
{
    [Key]
    public Guid AttachmentId { get; set; } = Guid.NewGuid();

    public Guid CardId { get; set; }

    public Guid UploaderId { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    // Full URL to the stored file
    [Required, MaxLength(2048)]
    public string FileUrl { get; set; } = string.Empty;

    // MIME type e.g. "image/png", "application/pdf"
    [MaxLength(100)]
    public string? FileType { get; set; }

    public long SizeKb { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
