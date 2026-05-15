namespace FlowStack.LabelService.DTOs;
public class ChecklistProgressResponse
{
    public Guid CardId { get; set; }
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public double Percentage => TotalItems == 0 ? 0 : Math.Round((double)CompletedItems / TotalItems * 100, 2);
}
