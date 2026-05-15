using System.ComponentModel.DataAnnotations;
using FlowStack.Workspace.Models;

namespace FlowStack.Workspace.DTOs;
public class AddMemberRequest
{
    [Required]
    public Guid UserId { get; set; }

    public WorkspaceMemberRole Role { get; set; } = WorkspaceMemberRole.Member;
}