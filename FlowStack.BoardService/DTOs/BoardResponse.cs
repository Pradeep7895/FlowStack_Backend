using System.ComponentModel.DataAnnotations;
using FlowStack.BoardService.Models;

namespace FlowStack.BoardService.DTOs;

public class BoardResponse
{
    public Guid BoardId { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Background { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public Guid CreatedById { get; set; }
    public bool IsClosed { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static BoardResponse FromBoard(Board b) => new()
    {
        BoardId = b.BoardId,
        WorkspaceId = b.WorkspaceId,
        Name = b.Name,
        Description = b.Description,
        Background = b.Background,
        Visibility = b.Visibility.ToString(),
        CreatedById = b.CreatedById,
        IsClosed = b.IsClosed,
        MemberCount = b.Members?.Count ?? 0,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}