using FlowStack.LabelService.Models;

namespace FlowStack.LabelService.Repositories;

public interface ILabelRepository
{
    // Labels 
    Task<Label?> GetLabelByIdAsync(Guid labelId);
    Task<IEnumerable<Label>> GetLabelsByBoardIdAsync(Guid boardId);
    Task<Label> CreateLabelAsync(Label label);
    Task<Label> UpdateLabelAsync(Label label);
    Task DeleteLabelAsync(Guid labelId);

    // Card Labels 
    Task<IEnumerable<Label>> GetLabelsForCardAsync(Guid cardId);
    Task AddLabelToCardAsync(Guid cardId, Guid labelId);
    Task RemoveLabelFromCardAsync(Guid cardId, Guid labelId);

    // Checklists 
    Task<Checklist?> GetChecklistByIdAsync(Guid checklistId);
    Task<IEnumerable<Checklist>> GetChecklistsByCardIdAsync(Guid cardId);
    Task<Checklist> CreateChecklistAsync(Checklist checklist);
    Task DeleteChecklistAsync(Guid checklistId);

    // Checklist Items 
    Task<ChecklistItem?> GetChecklistItemByIdAsync(Guid itemId);
    Task<ChecklistItem> AddItemAsync(ChecklistItem item);
    Task<ChecklistItem> UpdateItemAsync(ChecklistItem item);
    Task DeleteItemAsync(Guid itemId);

    // Progress Computation 
    Task<(int Total, int Completed)> GetChecklistProgressForCardAsync(Guid cardId);
}
