using System.ComponentModel.DataAnnotations;

namespace FlowStack.ListService.DTOs;

public class UpdateListRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }
}