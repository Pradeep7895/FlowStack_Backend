using FlowStack.ListService.Models;

namespace FlowStack.ListService.Repositories;

public interface IListRepository
{
    Task<TaskList?> FindByListIdAsync(Guid listId);
    Task<TaskList?> FindByListIdIncludingArchivedAsync(Guid listId);
    Task<IEnumerable<TaskList>> FindByBoardIdAsync(Guid boardId);
    Task<IEnumerable<TaskList>> FindByBoardIdOrderByPositionAsync(Guid boardId);
    Task<IEnumerable<TaskList>> FindByBoardIdAndIsArchivedAsync(Guid boardId, bool isArchived);
    Task<int> CountByBoardIdAsync(Guid boardId);
    Task<int> FindMaxPositionByBoardIdAsync(Guid boardId);
    Task<TaskList> CreateAsync(TaskList list);
    Task<TaskList> UpdateAsync(TaskList list);
    Task BatchUpdatePositionsAsync(IEnumerable<TaskList> lists);
    Task DeleteByListIdAsync(Guid listId);
}