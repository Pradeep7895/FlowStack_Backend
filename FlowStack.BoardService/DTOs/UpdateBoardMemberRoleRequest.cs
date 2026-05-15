using System.ComponentModel.DataAnnotations;
using FlowStack.BoardService.Models;

namespace FlowStack.BoardService.DTOs;

public class UpdateBoardMemberRoleRequest
{
    [Required]
    public BoardMemberRole Role { get; set; }
}