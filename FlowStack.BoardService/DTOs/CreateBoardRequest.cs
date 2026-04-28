using System.ComponentModel.DataAnnotations;
using FlowStack.BoardService.Models;

namespace FlowStack.BoardService.DTOs;

public class CreateBoardRequest
{
    [Required]
    public Guid WorkspaceId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Background { get; set; }

    public BoardVisibility Visibility { get; set; } = BoardVisibility.Private;
}
