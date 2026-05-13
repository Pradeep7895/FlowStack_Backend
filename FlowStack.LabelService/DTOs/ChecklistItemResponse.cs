namespace FlowStack.LabelService.DTOs;
public class ChecklistItemResponse
{
    public Guid ItemId { get; set; }
    public Guid ChecklistId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateOnly? DueDate { get; set; }
    public int Position { get; set; }
}