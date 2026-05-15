using FlowStack.ListService.Data;
using FlowStack.ListService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.ListService.Repositories;

public class ListRepository : IListRepository
{
    private readonly ListDbContext _db;

    public ListRepository(ListDbContext db)
    {
        _db = db;
    }

    // Single list lookups 
    
    public async Task<TaskList?> FindByListIdAsync(Guid listId) =>
        // Global query filter active — only returns non-archived
        await _db.TaskLists.FirstOrDefaultAsync(l => l.ListId == listId);

    public async Task<TaskList?> FindByListIdIncludingArchivedAsync(Guid listId) =>
        // IgnoreQueryFilters bypasses the IsArchived = false filter
        await _db.TaskLists.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.ListId == listId);

    // Board-scoped queries 

    public async Task<IEnumerable<TaskList>> FindByBoardIdAsync(Guid boardId) =>
        await _db.TaskLists.Where(l => l.BoardId == boardId).ToListAsync();

    public async Task<IEnumerable<TaskList>> FindByBoardIdOrderByPositionAsync(Guid boardId) =>
        await _db.TaskLists.Where(l => l.BoardId == boardId).OrderBy(l => l.Position).ToListAsync();

    public async Task<IEnumerable<TaskList>> FindByBoardIdAndIsArchivedAsync(
        Guid boardId, bool isArchived) =>
        await _db.TaskLists.IgnoreQueryFilters().Where(l => l.BoardId == boardId && l.IsArchived == isArchived)
                .OrderBy(l => l.Position)
                .ToListAsync();

    // Aggregates 

    public async Task<int> CountByBoardIdAsync(Guid boardId) =>
        await _db.TaskLists.CountAsync(l => l.BoardId == boardId);

    public async Task<int> FindMaxPositionByBoardIdAsync(Guid boardId)
    {
        var lists = await _db.TaskLists.Where(l => l.BoardId == boardId).ToListAsync();
        return lists.Any() ? lists.Max(l => l.Position) : 0;
    }

    // Write operations 

    public async Task<TaskList> CreateAsync(TaskList list)
    {
        _db.TaskLists.Add(list);
        await _db.SaveChangesAsync();
        return list;
    }

    public async Task<TaskList> UpdateAsync(TaskList list)
    {
        list.UpdatedAt = DateTime.UtcNow;
        _db.TaskLists.Update(list);
        await _db.SaveChangesAsync();
        return list;
    }

    public async Task BatchUpdatePositionsAsync(IEnumerable<TaskList> lists)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var list in lists)
            {
                list.UpdatedAt = DateTime.UtcNow;
                _db.Entry(list).Property(l => l.Position).IsModified  = true;
                _db.Entry(list).Property(l => l.UpdatedAt).IsModified = true;
            }
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteByListIdAsync(Guid listId)
    {
        // Must bypass filter in case we're deleting an archived list
        var list = await _db.TaskLists.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.ListId == listId);
        if (list is not null)
        {
            _db.TaskLists.Remove(list);
            await _db.SaveChangesAsync();
        }
    }
}