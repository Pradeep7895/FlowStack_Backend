using System.ComponentModel.DataAnnotations;
using FlowStack.BoardService.Models;

namespace FlowStack.BoardService.DTOs;

public class BoardMemberResponse
{
    public Guid BoardMemberId { get; set; }
    public Guid BoardId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }

    public static BoardMemberResponse FromMember(BoardMember m) => new()
    {
        BoardMemberId = m.BoardMemberId,
        BoardId = m.BoardId,
        UserId = m.UserId,
        Role = m.Role.ToString(),
        AddedAt = m.AddedAt
    };
}