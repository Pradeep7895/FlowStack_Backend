using FlowStack.LabelService.Data;
using FlowStack.LabelService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.LabelService.Repositories;

public class LabelRepository : ILabelRepository
{
    private readonly LabelDbContext _db;

    public LabelRepository(LabelDbContext db)
    {
        _db = db;
    }

    // Labels 

    public async Task<Label?> GetLabelByIdAsync(Guid labelId) =>
        await _db.Labels.FirstOrDefaultAsync(l => l.LabelId == labelId);

    public async Task<IEnumerable<Label>> GetLabelsByBoardIdAsync(Guid boardId) =>
        await _db.Labels
            .Where(l => l.BoardId == boardId)
            .OrderBy(l => l.Name)
            .ToListAsync();

    public async Task<Label> CreateLabelAsync(Label label)
    {
        _db.Labels.Add(label);
        await _db.SaveChangesAsync();
        return label;
    }

    public async Task<Label> UpdateLabelAsync(Label label)
    {
        _db.Labels.Update(label);
        await _db.SaveChangesAsync();
        return label;
    }

    public async Task DeleteLabelAsync(Guid labelId)
    {
        var label = await GetLabelByIdAsync(labelId);
        if (label is not null)
        {
            _db.Labels.Remove(label);
            await _db.SaveChangesAsync();
        }
    }

    // Card Labels 

    public async Task<IEnumerable<Label>> GetLabelsForCardAsync(Guid cardId) =>
        await _db.CardLabels
            .Include(cl => cl.Label)
            .Where(cl => cl.CardId == cardId && cl.Label != null)
            .Select(cl => cl.Label!)
            .OrderBy(l => l.Name)
            .ToListAsync();

    public async Task AddLabelToCardAsync(Guid cardId, Guid labelId)
    {
        var exists = await _db.CardLabels.AnyAsync(cl => cl.CardId == cardId && cl.LabelId == labelId);
        if (!exists)
        {
            _db.CardLabels.Add(new CardLabel { CardId = cardId, LabelId = labelId });
            await _db.SaveChangesAsync();
        }
    }

    public async Task RemoveLabelFromCardAsync(Guid cardId, Guid labelId)
    {
        var mapping = await _db.CardLabels.FirstOrDefaultAsync(cl => cl.CardId == cardId && cl.LabelId == labelId);
        if (mapping is not null)
        {
            _db.CardLabels.Remove(mapping);
            await _db.SaveChangesAsync();
        }
    }

    // Checklists 

    public async Task<Checklist?> GetChecklistByIdAsync(Guid checklistId) =>
        await _db.Checklists
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.ChecklistId == checklistId);

    public async Task<IEnumerable<Checklist>> GetChecklistsByCardIdAsync(Guid cardId) =>
        await _db.Checklists
            .Include(c => c.Items.OrderBy(i => i.Position))
            .Where(c => c.CardId == cardId)
            .OrderBy(c => c.Position)
            .ToListAsync();

    public async Task<Checklist> CreateChecklistAsync(Checklist checklist)
    {
        var maxPos = await _db.Checklists
            .Where(c => c.CardId == checklist.CardId)
            .Select(c => (int?)c.Position)
            .MaxAsync() ?? 0;

        checklist.Position = maxPos + 1;
        _db.Checklists.Add(checklist);
        await _db.SaveChangesAsync();
        return checklist;
    }

    public async Task DeleteChecklistAsync(Guid checklistId)
    {
        var checklist = await _db.Checklists.FirstOrDefaultAsync(c => c.ChecklistId == checklistId);
        if (checklist is not null)
        {
            _db.Checklists.Remove(checklist);
            await _db.SaveChangesAsync();
        }
    }

    // Checklist Items 

    public async Task<ChecklistItem?> GetChecklistItemByIdAsync(Guid itemId) =>
        await _db.ChecklistItems
            .Include(i => i.Checklist)
            .FirstOrDefaultAsync(i => i.ItemId == itemId);

    public async Task<ChecklistItem> AddItemAsync(ChecklistItem item)
    {
        var maxPos = await _db.ChecklistItems
            .Where(i => i.ChecklistId == item.ChecklistId)
            .Select(i => (int?)i.Position)
            .MaxAsync() ?? 0;

        item.Position = maxPos + 1;
        _db.ChecklistItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<ChecklistItem> UpdateItemAsync(ChecklistItem item)
    {
        _db.ChecklistItems.Update(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task DeleteItemAsync(Guid itemId)
    {
        var item = await _db.ChecklistItems.FirstOrDefaultAsync(i => i.ItemId == itemId);
        if (item is not null)
        {
            _db.ChecklistItems.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    // Progress Computation 

    public async Task<(int Total, int Completed)> GetChecklistProgressForCardAsync(Guid cardId)
    {
        var items = await _db.ChecklistItems
            .Include(i => i.Checklist)
            .Where(i => i.Checklist != null && i.Checklist.CardId == cardId)
            .ToListAsync();

        return (items.Count, items.Count(i => i.IsCompleted));
    }
}
