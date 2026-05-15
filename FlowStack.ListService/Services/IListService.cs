using FlowStack.ListService.DTOs;

namespace FlowStack.ListService.Services;

public interface IListService
{
    // CRUD
    Task<TaskListResponse> CreateListAsync(Guid requesterId, CreateListRequest request);
    Task<TaskListResponse> GetListByIdAsync(Guid listId, Guid requesterId);
    Task<IEnumerable<TaskListResponse>> GetListsByBoardAsync(Guid boardId, Guid requesterId);
    Task<TaskListResponse> UpdateListAsync(Guid listId, Guid requesterId, UpdateListRequest request);

    // Position management 
    Task<IEnumerable<TaskListResponse>> ReorderListsAsync(Guid requesterId, ReorderListsRequest request);

    // Archival
    Task<TaskListResponse> ArchiveListAsync(Guid listId, Guid requesterId);
    Task<TaskListResponse> UnarchiveListAsync(Guid listId, Guid requesterId);
    Task<IEnumerable<TaskListResponse>> GetArchivedListsAsync(Guid boardId, Guid requesterId);

    //Hard delete
    Task DeleteListAsync(Guid listId, Guid requesterId);

    // Board transfer
    Task<TaskListResponse> MoveListAsync(Guid listId, Guid requesterId, MoveListRequest request);
}