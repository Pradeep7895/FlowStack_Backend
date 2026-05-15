using System.ComponentModel.DataAnnotations;
using FlowStack.Workspace.Models;

namespace FlowStack.Workspace.DTOs;

public class UpdateMemberRoleRequest
{
    [Required]
    public WorkspaceMemberRole Role { get; set; }
}