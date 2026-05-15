using System.ComponentModel.DataAnnotations;

namespace FlowStack.CommentService.DTOs;

public class AddAttachmentRequest
{
    [Required]
    public Guid CardId { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(2048)]
    public string FileUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FileType { get; set; }

    public long SizeKb { get; set; }
}