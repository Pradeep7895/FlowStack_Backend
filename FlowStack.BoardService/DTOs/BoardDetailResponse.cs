using System.ComponentModel.DataAnnotations;
using FlowStack.BoardService.Models;

namespace FlowStack.BoardService.DTOs;

public class BoardDetailResponse : BoardResponse
{
    public IEnumerable<BoardMemberResponse> Members { get; set; }
        = Enumerable.Empty<BoardMemberResponse>();

    public static BoardDetailResponse FromBoardWithMembers(Board b) => new()
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
        UpdatedAt = b.UpdatedAt,
        Members = b.Members?.Select(BoardMemberResponse.FromMember)
                        ?? Enumerable.Empty<BoardMemberResponse>()
    };
}