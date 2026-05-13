namespace FlowStack.LabelService.DTOs;
public class LabelResponse
{
    public Guid LabelId { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}