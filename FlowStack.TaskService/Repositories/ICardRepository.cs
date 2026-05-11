using FlowStack.TaskService.Models;

namespace FlowStack.TaskService.Repositories;

public interface ICardRepository
{
    Task<Card?> FindByCardIdAsync(Guid cardId);
    Task<Card?> FindByCardIdIncludingArchivedAsync(Guid cardId);

    Task<IEnumerable<Card>> FindByListIdAsync(Guid listId);
    Task<IEnumerable<Card>> FindByListIdOrderByPositionAsync(Guid listId);
    Task<int> CountByListIdAsync(Guid listId);
    Task<int> FindMaxPositionByListIdAsync(Guid listId);

    Task<IEnumerable<Card>> FindByBoardIdAsync(Guid boardId);

    Task<IEnumerable<Card>> FindByAssigneeIdAsync(Guid assigneeId);

    Task<IEnumerable<Card>> FindByStatusAsync(Guid boardId, string status);
    Task<IEnumerable<Card>> FindByPriorityAsync(Guid boardId, string priority);
    Task<IEnumerable<Card>> FindByDueDateBeforeAsync(DateTime dueDate);

    Task<IEnumerable<Card>> FindArchivedByListIdAsync(Guid listId);
    Task<IEnumerable<Card>> FindArchivedByBoardIdAsync(Guid boardId);

    Task<Card> CreateAsync(Card card);
    Task<Card> UpdateAsync(Card card);
    Task BatchUpdatePositionsAsync(IEnumerable<Card> cards);
    Task DeleteByCardIdAsync(Guid cardId);
}
