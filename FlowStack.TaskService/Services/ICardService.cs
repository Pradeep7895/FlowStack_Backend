using FlowStack.TaskService.DTOs;

namespace FlowStack.TaskService.Services;

/// <summary>
/// Declares all card CRUD, move/reorder, archival,
/// assignment, priority, and overdue detection operations.
/// </summary>
public interface ICardService
{
    // CRUD
    Task<CardResponse> CreateCardAsync(Guid requesterId, CreateCardRequest request);
    Task<CardResponse> GetCardByIdAsync(Guid cardId, Guid requesterId);
    Task<IEnumerable<CardResponse>> GetCardsByListAsync(Guid listId, Guid requesterId);
    Task<IEnumerable<CardResponse>> GetCardsByBoardAsync(Guid boardId, Guid requesterId);
    Task<IEnumerable<CardResponse>> GetCardsByAssigneeAsync(Guid assigneeId);
    Task<CardResponse> UpdateCardAsync(Guid cardId, Guid requesterId, UpdateCardRequest request);

    // Move / Reorder
    Task<CardResponse> MoveCardAsync(Guid cardId, Guid requesterId, MoveCardRequest request);
    Task<IEnumerable<CardResponse>> ReorderCardsAsync(Guid requesterId, ReorderCardsRequest request);

    // Archive / Unarchive
    Task<CardResponse> ArchiveCardAsync(Guid cardId, Guid requesterId);
    Task<CardResponse> UnarchiveCardAsync(Guid cardId, Guid requesterId);

    // Hard delete
    Task DeleteCardAsync(Guid cardId, Guid requesterId);

    // Assignment & Priority
    Task<CardResponse> SetAssigneeAsync(Guid cardId, Guid requesterId, SetAssigneeRequest request);
    Task<CardResponse> SetPriorityAsync(Guid cardId, Guid requesterId, SetPriorityRequest request);

    // Overdue detection
    Task<IEnumerable<CardResponse>> GetOverdueCardsAsync(Guid requesterId);
}
