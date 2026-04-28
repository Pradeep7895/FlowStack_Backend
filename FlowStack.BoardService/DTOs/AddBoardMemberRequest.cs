using System.ComponentModel.DataAnnotations;
using FlowStack.BoardService.Models;

namespace FlowStack.BoardService.DTOs;

public class AddBoardMemberRequest
{
    [Required]
    public Guid UserId { get; set; }

    public BoardMemberRole Role { get; set; } = BoardMemberRole.Member;
}