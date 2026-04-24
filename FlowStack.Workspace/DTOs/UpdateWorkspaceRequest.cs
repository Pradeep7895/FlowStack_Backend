using System.ComponentModel.DataAnnotations;
using FlowStack.Workspace.Models;

namespace FlowStack.Workspace.DTOs;

public class UpdateWorkspaceRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public WorkspaceVisibility? Visibility { get; set; }
}