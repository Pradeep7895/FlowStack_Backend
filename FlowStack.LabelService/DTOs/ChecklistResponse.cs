namespace FlowStack.LabelService.DTOs;
public class ChecklistResponse
{
    public Guid ChecklistId { get; set; }
    public Guid CardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Position { get; set; }
    public List<ChecklistItemResponse> Items { get; set; } = new();
}