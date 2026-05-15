using FlowStack.LabelService.DTOs;
using FlowStack.LabelService.Helpers;
using FlowStack.LabelService.Models;
using FlowStack.LabelService.Repositories;

namespace FlowStack.LabelService.Services;

public class LabelServiceImpl : ILabelService
{
    private readonly ILabelRepository _repo;
    private readonly TaskClient _taskClient;
    private readonly BoardClient _boardClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LabelServiceImpl(
        ILabelRepository repo, 
        TaskClient taskClient, 
        BoardClient boardClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _taskClient = taskClient;
        _boardClient = boardClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private bool IsPlatformAdmin() =>
        _httpContextAccessor.HttpContext?.User.IsInRole("PlatformAdmin") ?? false;

    private async Task<BoardAccessResult> GetBoardAccessWithBypassAsync(Guid boardId, Guid userId)
    {
        if (IsPlatformAdmin())
        {
            return new BoardAccessResult
            {
                IsMember = true,
                IsAdminOrCreator = true,
                IsObserver = false,
                IsClosed = false
            };
        }
        return await _boardClient.GetBoardAccessAsync(boardId, userId);
    }

    // Labels 

    public async Task<LabelResponse> CreateLabelAsync(Guid requesterId, string authHeader, CreateLabelRequest request)
    {
        var access = await GetBoardAccessWithBypassAsync(request.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to create labels.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot create labels on a closed board.");

        var label = new Label
        {
            BoardId = request.BoardId,
            Name = request.Name,
            Color = request.Color
        };

        label = await _repo.CreateLabelAsync(label);

        return new LabelResponse
        {
            LabelId = label.LabelId,
            BoardId = label.BoardId,
            Name = label.Name,
            Color = label.Color
        };
    }

    public async Task<IEnumerable<LabelResponse>> GetLabelsByBoardAsync(Guid boardId, Guid requesterId)
    {
        var access = await GetBoardAccessWithBypassAsync(boardId, requesterId);

        // As long as they are a member (including observer), they can view labels
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to view labels.");

        var labels = await _repo.GetLabelsByBoardIdAsync(boardId);
        return labels.Select(l => new LabelResponse
        {
            LabelId = l.LabelId,
            BoardId = l.BoardId,
            Name = l.Name,
            Color = l.Color
        });
    }

    public async Task<LabelResponse> UpdateLabelAsync(Guid labelId, Guid requesterId, string authHeader, UpdateLabelRequest request)
    {
        var label = await _repo.GetLabelByIdAsync(labelId)
            ?? throw new KeyNotFoundException("Label not found.");

        var access = await GetBoardAccessWithBypassAsync(label.BoardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to update labels.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot update labels on a closed board.");

        label.Name = request.Name;
        label.Color = request.Color;

        label = await _repo.UpdateLabelAsync(label);

        return new LabelResponse
        {
            LabelId = label.LabelId,
            BoardId = label.BoardId,
            Name = label.Name,
            Color = label.Color
        };
    }

    public async Task DeleteLabelAsync(Guid labelId, Guid requesterId, string authHeader)
    {
        var label = await _repo.GetLabelByIdAsync(labelId)
            ?? throw new KeyNotFoundException("Label not found.");

        var access = await GetBoardAccessWithBypassAsync(label.BoardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to delete labels.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot delete labels on a closed board.");

        await _repo.DeleteLabelAsync(labelId);
    }

    // Card Labels 

    public async Task<IEnumerable<LabelResponse>> GetLabelsForCardAsync(Guid cardId, Guid requesterId, string authHeader)
    {
        var boardId = await _taskClient.GetBoardIdForCardAsync(cardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var labels = await _repo.GetLabelsForCardAsync(cardId);

        return labels.Select(l => new LabelResponse
        {
            LabelId = l.LabelId,
            BoardId = l.BoardId,
            Name = l.Name,
            Color = l.Color
        });
    }

    public async Task AddLabelToCardAsync(Guid cardId, Guid labelId, Guid requesterId, string authHeader)
    {
        var label = await _repo.GetLabelByIdAsync(labelId)
            ?? throw new KeyNotFoundException("Label not found.");

        var boardId = await _taskClient.GetBoardIdForCardAsync(cardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        if (label.BoardId != boardId)
            throw new InvalidOperationException("Label and Card do not belong to the same board.");

        var access = await GetBoardAccessWithBypassAsync(boardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to add labels to cards.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot modify a closed board.");

        await _repo.AddLabelToCardAsync(cardId, labelId);
    }

    public async Task RemoveLabelFromCardAsync(Guid cardId, Guid labelId, Guid requesterId, string authHeader)
    {
        var boardId = await _taskClient.GetBoardIdForCardAsync(cardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await GetBoardAccessWithBypassAsync(boardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to remove labels from cards.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot modify a closed board.");

        await _repo.RemoveLabelFromCardAsync(cardId, labelId);
    }

    // Checklists 

    public async Task<ChecklistResponse> CreateChecklistAsync(Guid requesterId, string authHeader, CreateChecklistRequest request)
    {
        var boardId = await _taskClient.GetBoardIdForCardAsync(request.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await GetBoardAccessWithBypassAsync(boardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to create checklists.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot modify a closed board.");

        var checklist = new Checklist
        {
            CardId = request.CardId,
            Title = request.Title
        };

        checklist = await _repo.CreateChecklistAsync(checklist);

        return new ChecklistResponse
        {
            ChecklistId = checklist.ChecklistId,
            CardId = checklist.CardId,
            Title = checklist.Title,
            Position = checklist.Position
        };
    }

    public async Task<IEnumerable<ChecklistResponse>> GetChecklistsByCardAsync(Guid cardId, Guid requesterId, string authHeader)
    {
        var boardId = await _taskClient.GetBoardIdForCardAsync(cardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var checklists = await _repo.GetChecklistsByCardIdAsync(cardId);

        return checklists.Select(c => new ChecklistResponse
        {
            ChecklistId = c.ChecklistId,
            CardId = c.CardId,
            Title = c.Title,
            Position = c.Position,
            Items = c.Items.Select(i => new ChecklistItemResponse
            {
                ItemId = i.ItemId,
                ChecklistId = i.ChecklistId,
                Text = i.Text,
                IsCompleted = i.IsCompleted,
                AssigneeId = i.AssigneeId,
                DueDate = i.DueDate,
                Position = i.Position
            }).ToList()
        });
    }

    public async Task DeleteChecklistAsync(Guid checklistId, Guid requesterId, string authHeader)
    {
        var checklist = await _repo.GetChecklistByIdAsync(checklistId)
            ?? throw new KeyNotFoundException("Checklist not found.");

        var boardId = await _taskClient.GetBoardIdForCardAsync(checklist.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await GetBoardAccessWithBypassAsync(boardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to delete checklists.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot modify a closed board.");

        await _repo.DeleteChecklistAsync(checklistId);
    }

    // Checklist Items 

    public async Task<ChecklistItemResponse> AddItemAsync(Guid checklistId, Guid requesterId, string authHeader, AddChecklistItemRequest request)
    {
        var checklist = await _repo.GetChecklistByIdAsync(checklistId)
            ?? throw new KeyNotFoundException("Checklist not found.");

        var boardId = await _taskClient.GetBoardIdForCardAsync(checklist.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await GetBoardAccessWithBypassAsync(boardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to add items.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot modify a closed board.");

        var item = new ChecklistItem
        {
            ChecklistId = checklistId,
            Text = request.Text
        };

        item = await _repo.AddItemAsync(item);

        return new ChecklistItemResponse
        {
            ItemId = item.ItemId,
            ChecklistId = item.ChecklistId,
            Text = item.Text,
            IsCompleted = item.IsCompleted,
            Position = item.Position
        };
    }

    public async Task<ChecklistItemResponse> ToggleItemAsync(Guid itemId, Guid requesterId, string authHeader, ToggleItemRequest request)
    {
        var item = await _repo.GetChecklistItemByIdAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found.");

        var boardId = await _taskClient.GetBoardIdForCardAsync(item.Checklist!.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await GetBoardAccessWithBypassAsync(boardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to toggle items.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot modify a closed board.");

        item.IsCompleted = request.IsCompleted;
        item = await _repo.UpdateItemAsync(item);

        return new ChecklistItemResponse
        {
            ItemId = item.ItemId,
            ChecklistId = item.ChecklistId,
            Text = item.Text,
            IsCompleted = item.IsCompleted,
            AssigneeId = item.AssigneeId,
            DueDate = item.DueDate,
            Position = item.Position
        };
    }

    public async Task<ChecklistItemResponse> SetItemAssigneeAsync(Guid itemId, Guid requesterId, string authHeader, SetItemAssigneeRequest request)
    {
        var item = await _repo.GetChecklistItemByIdAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found.");

        var boardId = await _taskClient.GetBoardIdForCardAsync(item.Checklist!.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await GetBoardAccessWithBypassAsync(boardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to assign items.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot modify a closed board.");

        item.AssigneeId = request.AssigneeId;
        item = await _repo.UpdateItemAsync(item);

        return new ChecklistItemResponse
        {
            ItemId = item.ItemId,
            ChecklistId = item.ChecklistId,
            Text = item.Text,
            IsCompleted = item.IsCompleted,
            AssigneeId = item.AssigneeId,
            DueDate = item.DueDate,
            Position = item.Position
        };
    }

    public async Task<ChecklistItemResponse> SetItemDueDateAsync(Guid itemId, Guid requesterId, string authHeader, SetItemDueDateRequest request)
    {
        var item = await _repo.GetChecklistItemByIdAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found.");

        var boardId = await _taskClient.GetBoardIdForCardAsync(item.Checklist!.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await GetBoardAccessWithBypassAsync(boardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to set item due dates.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot modify a closed board.");

        item.DueDate = request.DueDate;
        item = await _repo.UpdateItemAsync(item);

        return new ChecklistItemResponse
        {
            ItemId = item.ItemId,
            ChecklistId = item.ChecklistId,
            Text = item.Text,
            IsCompleted = item.IsCompleted,
            AssigneeId = item.AssigneeId,
            DueDate = item.DueDate,
            Position = item.Position
        };
    }

    public async Task DeleteItemAsync(Guid itemId, Guid requesterId, string authHeader)
    {
        var item = await _repo.GetChecklistItemByIdAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found.");

        var boardId = await _taskClient.GetBoardIdForCardAsync(item.Checklist!.CardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var access = await GetBoardAccessWithBypassAsync(boardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to delete items.");
        if (access.IsClosed)
            throw new InvalidOperationException("Cannot modify a closed board.");

        await _repo.DeleteItemAsync(itemId);
    }

    // Progress Computation 

    public async Task<ChecklistProgressResponse> GetChecklistProgressAsync(Guid cardId, Guid requesterId, string authHeader)
    {
        var boardId = await _taskClient.GetBoardIdForCardAsync(cardId, authHeader)
            ?? throw new KeyNotFoundException("Card not found or access denied.");

        var (total, completed) = await _repo.GetChecklistProgressForCardAsync(cardId);

        return new ChecklistProgressResponse
        {
            CardId = cardId,
            TotalItems = total,
            CompletedItems = completed
        };
    }
}
