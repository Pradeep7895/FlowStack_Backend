using FlowStack.ListService.DTOs;
using FlowStack.ListService.Helpers;
using FlowStack.ListService.Models;
using FlowStack.ListService.Repositories;

namespace FlowStack.ListService.Services;

public class ListServiceImpl : IListService
{
    private readonly IListRepository _repo;
    private readonly BoardClient _boardClient;

    public ListServiceImpl(IListRepository repo, BoardClient boardClient)
    {
        _repo = repo;
        _boardClient = boardClient;
    }

    // CRUD 

    public async Task<TaskListResponse> CreateListAsync(Guid requesterId, CreateListRequest request)
    {
        var access = await _boardClient.GetBoardAccessAsync(request.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to create lists.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot add lists to a closed board.");

        // Auto-assign position as max + 1 (append to the right)
        var maxPosition = await _repo.FindMaxPositionByBoardIdAsync(request.BoardId);

        var list = new TaskList
        {
            BoardId = request.BoardId,
            Name = request.Name,
            Color = request.Color,
            Position = maxPosition + 1
        };

        list = await _repo.CreateAsync(list);
        return TaskListResponse.FromTaskList(list);
    }

    public async Task<TaskListResponse> GetListByIdAsync(Guid listId, Guid requesterId)
    {
        var list = await RequireListAsync(listId);
        var access = await _boardClient.GetBoardAccessAsync(list.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to view this list.");

        return TaskListResponse.FromTaskList(list);
    }

    public async Task<IEnumerable<TaskListResponse>> GetListsByBoardAsync(Guid boardId, Guid requesterId)
    {
        var access = await _boardClient.GetBoardAccessAsync(boardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to view lists.");

        var lists = await _repo.FindByBoardIdOrderByPositionAsync(boardId);
        return lists.Select(TaskListResponse.FromTaskList);
    }

    public async Task<TaskListResponse> UpdateListAsync(Guid listId, Guid requesterId, UpdateListRequest request)
    {
        var list = await RequireListAsync(listId);
        var access = await _boardClient.GetBoardAccessAsync(list.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to update lists.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot update lists on a closed board.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot modify lists.");

        if (request.Name  is not null) list.Name = request.Name;
        if (request.Color is not null) list.Color = request.Color;

        list = await _repo.UpdateAsync(list);
        return TaskListResponse.FromTaskList(list);
    }

    // Position management 
    public async Task<IEnumerable<TaskListResponse>> ReorderListsAsync(Guid requesterId, ReorderListsRequest request)
    {
        var access = await _boardClient.GetBoardAccessAsync(request.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to reorder lists.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot reorder lists on a closed board.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot reorder lists.");

        // Load all active lists currently on the board
        var currentLists = (await _repo.FindByBoardIdOrderByPositionAsync(request.BoardId)).ToList();

        var currentIds = currentLists.Select(l => l.ListId).ToHashSet();
        var requestedIds = request.OrderedListIds.ToHashSet();

        // Validate: the client must send exactly the same set of list IDs
        if (!currentIds.SetEquals(requestedIds))
            throw new InvalidOperationException("The reorder request must include all active lists on the board — " +
                "no extras and no missing lists.");

        // Build a lookup for fast access
        var listLookup = currentLists.ToDictionary(l => l.ListId);

        // Assign new positions based on order in request (1-indexed)
        var listsToUpdate = request.OrderedListIds
            .Select((listId, index) =>
            {
                var list = listLookup[listId];
                list.Position = index + 1;
                return list;
            })
            .ToList();

        // Atomic batch update — all positions or none
        await _repo.BatchUpdatePositionsAsync(listsToUpdate);

        return listsToUpdate.OrderBy(l => l.Position).Select(TaskListResponse.FromTaskList);
    }

    // Archival 

    public async Task<TaskListResponse> ArchiveListAsync(Guid listId, Guid requesterId)
    {
        var list = await RequireListAsync(listId);
        var access = await _boardClient.GetBoardAccessAsync(list.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to archive lists.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot archive lists.");

        if (list.IsArchived)
            throw new InvalidOperationException("List is already archived.");

        list.IsArchived = true;
        list = await _repo.UpdateAsync(list);

        // Compact positions of remaining active lists so there are no gaps
        await CompactPositionsAsync(list.BoardId);

        return TaskListResponse.FromTaskList(list);
    }

    public async Task<TaskListResponse> UnarchiveListAsync(Guid listId, Guid requesterId)
    {
        // Must use IgnoreQueryFilters variant to find archived list
        var list = await _repo.FindByListIdIncludingArchivedAsync(listId)
            ?? throw new KeyNotFoundException($"List {listId} not found.");

        if (!list.IsArchived)
            throw new InvalidOperationException("List is not archived.");

        var access = await _boardClient.GetBoardAccessAsync(list.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to unarchive lists.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot unarchive lists.");

        list.IsArchived = false;

        // Append to the end of current active lists
        var maxPosition = await _repo.FindMaxPositionByBoardIdAsync(list.BoardId);
        list.Position = maxPosition + 1;

        list = await _repo.UpdateAsync(list);
        return TaskListResponse.FromTaskList(list);
    }

    public async Task<IEnumerable<TaskListResponse>> GetArchivedListsAsync(Guid boardId, Guid requesterId)
    {
        var access = await _boardClient.GetBoardAccessAsync(boardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to view archived lists.");

        var lists = await _repo.FindByBoardIdAndIsArchivedAsync(boardId, true);
        return lists.Select(TaskListResponse.FromTaskList);
    }

    // Hard delete 

    public async Task DeleteListAsync(Guid listId, Guid requesterId)
    {
        // Must find including archived — only archived lists can be hard-deleted
        var list = await _repo.FindByListIdIncludingArchivedAsync(listId)
            ?? throw new KeyNotFoundException($"List {listId} not found.");

        var access = await _boardClient.GetBoardAccessAsync(list.BoardId, requesterId);

        if (!access.IsAdminOrCreator)
            throw new UnauthorizedAccessException("Only board Admins or the board creator can permanently delete lists.");

        // Enforce: only archived lists can be permanently deleted
        // Active lists must be archived first (two-step deletion UX like Trello)
        if (!list.IsArchived)
            throw new InvalidOperationException("Only archived lists can be permanently deleted. Archive the list first.");

        await _repo.DeleteByListIdAsync(listId);
    }

    // Board transfer 

    public async Task<TaskListResponse> MoveListAsync(Guid listId, Guid requesterId, MoveListRequest request)
    {
        var list = await RequireListAsync(listId);

        // Requester must be Admin on the SOURCE board
        var sourceAccess = await _boardClient.GetBoardAccessAsync(list.BoardId, requesterId);
        if (!sourceAccess.IsAdminOrCreator)
            throw new UnauthorizedAccessException("Only board Admins or the creator can move lists between boards.");

        if (sourceAccess.IsClosed)
            throw new InvalidOperationException("Cannot move lists from a closed board.");

        // Requester must also be a member of the TARGET board
        var targetAccess = await _boardClient.GetBoardAccessAsync(request.TargetBoardId, requesterId);

        if (!targetAccess.IsMember)
            throw new UnauthorizedAccessException("You must be a member of the target board to move a list there.");

        if (targetAccess.IsClosed)
            throw new InvalidOperationException("Cannot move a list to a closed board.");

        // Both boards must be in the same workspace
        if (sourceAccess.WorkspaceId != targetAccess.WorkspaceId)
            throw new InvalidOperationException("Lists can only be moved between boards in the same workspace.");

        // Compact source board positions after removal
        await CompactPositionsAsync(list.BoardId);

        // Move to target board — append at end
        var targetMaxPosition = await _repo.FindMaxPositionByBoardIdAsync(request.TargetBoardId);
        list.BoardId = request.TargetBoardId;
        list.Position = targetMaxPosition + 1;

        list = await _repo.UpdateAsync(list);
        return TaskListResponse.FromTaskList(list);
    }

    // Private helpers 

    private async Task<TaskList> RequireListAsync(Guid listId) =>
        await _repo.FindByListIdAsync(listId)
            ?? throw new KeyNotFoundException($"List {listId} not found.");

    // After archiving or moving a list, renumbers remaining active lists
    // sequentially (1, 2, 3...) to eliminate gaps in Position values.
    private async Task CompactPositionsAsync(Guid boardId)
    {
        var activeLists = (await _repo.FindByBoardIdOrderByPositionAsync(boardId)).ToList();
        for (int i = 0; i < activeLists.Count; i++)
            activeLists[i].Position = i + 1;

        if (activeLists.Any())
            await _repo.BatchUpdatePositionsAsync(activeLists);
    }
}