using FlowStack.LabelService.DTOs;

namespace FlowStack.LabelService.Services;

public interface ILabelService
{
    // Labels 
    Task<LabelResponse> CreateLabelAsync(Guid requesterId, string authHeader, CreateLabelRequest request);
    Task<IEnumerable<LabelResponse>> GetLabelsByBoardAsync(Guid boardId, Guid requesterId);
    Task<LabelResponse> UpdateLabelAsync(Guid labelId, Guid requesterId, string authHeader, UpdateLabelRequest request);
    Task DeleteLabelAsync(Guid labelId, Guid requesterId, string authHeader);

    // Card Labels
    Task<IEnumerable<LabelResponse>> GetLabelsForCardAsync(Guid cardId, Guid requesterId, string authHeader);
    Task AddLabelToCardAsync(Guid cardId, Guid labelId, Guid requesterId, string authHeader);
    Task RemoveLabelFromCardAsync(Guid cardId, Guid labelId, Guid requesterId, string authHeader);

    // Checklists 
    Task<ChecklistResponse> CreateChecklistAsync(Guid requesterId, string authHeader, CreateChecklistRequest request);
    Task<IEnumerable<ChecklistResponse>> GetChecklistsByCardAsync(Guid cardId, Guid requesterId, string authHeader);
    Task DeleteChecklistAsync(Guid checklistId, Guid requesterId, string authHeader);

    // Checklist Items 
    Task<ChecklistItemResponse> AddItemAsync(Guid checklistId, Guid requesterId, string authHeader, AddChecklistItemRequest request);
    Task<ChecklistItemResponse> ToggleItemAsync(Guid itemId, Guid requesterId, string authHeader, ToggleItemRequest request);
    Task<ChecklistItemResponse> SetItemAssigneeAsync(Guid itemId, Guid requesterId, string authHeader, SetItemAssigneeRequest request);
    Task<ChecklistItemResponse> SetItemDueDateAsync(Guid itemId, Guid requesterId, string authHeader, SetItemDueDateRequest request);
    Task DeleteItemAsync(Guid itemId, Guid requesterId, string authHeader);

    // Progress Computation 
    Task<ChecklistProgressResponse> GetChecklistProgressAsync(Guid cardId, Guid requesterId, string authHeader);
}
