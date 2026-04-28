using System.ComponentModel.DataAnnotations;
using FlowStack.BoardService.Models;

namespace FlowStack.BoardService.DTOs;
public class BoardAccessResponse
{
    public bool IsMember { get; set; }
    public bool IsAdminOrCreator { get; set; }
    public bool IsObserver { get; set; }
    public bool IsClosed { get; set; }
    public Guid WorkspaceId { get; set; }
}