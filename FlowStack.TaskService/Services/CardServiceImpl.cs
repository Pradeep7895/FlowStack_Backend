using FlowStack.TaskService.DTOs;
using FlowStack.TaskService.Helpers;
using FlowStack.TaskService.Models;
using FlowStack.TaskService.Repositories;

namespace FlowStack.TaskService.Services;

public class CardServiceImpl : ICardService
{
    private readonly ICardRepository _repo;
    private readonly BoardClient _boardClient;

    private static readonly HashSet<string> ValidPriorities = new(StringComparer.OrdinalIgnoreCase)
        { "LOW", "MEDIUM", "HIGH", "CRITICAL" };

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "TO_DO", "IN_PROGRESS", "IN_REVIEW", "DONE" };

    public CardServiceImpl(ICardRepository repo, BoardClient boardClient)
    {
        _repo = repo;
        _boardClient = boardClient;
    }

    // CRUD 

    public async Task<CardResponse> CreateCardAsync(Guid requesterId, CreateCardRequest request)
    {
        var access = await _boardClient.GetBoardAccessAsync(request.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to create cards.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot add cards to a closed board.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot create cards.");

        var priority = request.Priority?.ToUpperInvariant() ?? "MEDIUM";
        if (!ValidPriorities.Contains(priority))
            throw new ArgumentException($"Invalid priority '{request.Priority}'. Must be one of: {string.Join(", ", ValidPriorities)}");

        // Auto-assign position as max + 1 (append to bottom)
        var maxPosition = await _repo.FindMaxPositionByListIdAsync(request.ListId);

        var card = new Card
        {
            ListId = request.ListId,
            BoardId = request.BoardId,
            Title = request.Title,
            Description = request.Description,
            Priority = priority,
            CoverColor = request.CoverColor,
            Position = maxPosition + 1,
            CreatedById = requesterId
        };

        card = await _repo.CreateAsync(card);
        return CardResponse.FromCard(card);
    }

    public async Task<CardResponse> GetCardByIdAsync(Guid cardId, Guid requesterId)
    {
        var card = await RequireCardAsync(cardId);
        var access = await _boardClient.GetBoardAccessAsync(card.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to view this card.");

        return CardResponse.FromCard(card);
    }

    public async Task<IEnumerable<CardResponse>> GetCardsByListAsync(Guid listId, Guid requesterId)
    {
        var cards = (await _repo.FindByListIdOrderByPositionAsync(listId)).ToList();
        if (!cards.Any()) return Enumerable.Empty<CardResponse>();

        // Use the boardId from the first card to check access
        var access = await _boardClient.GetBoardAccessAsync(cards.First().BoardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to view cards.");

        return cards.Select(CardResponse.FromCard);
    }

    public async Task<IEnumerable<CardResponse>> GetCardsByBoardAsync(Guid boardId, Guid requesterId)
    {
        var access = await _boardClient.GetBoardAccessAsync(boardId, requesterId);
        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to view cards.");

        var cards = await _repo.FindByBoardIdAsync(boardId);
        return cards.Select(CardResponse.FromCard);
    }

    public async Task<IEnumerable<CardResponse>> GetCardsByAssigneeAsync(Guid assigneeId)
    {
        var cards = await _repo.FindByAssigneeIdAsync(assigneeId);
        return cards.Select(CardResponse.FromCard);
    }

    public async Task<CardResponse> UpdateCardAsync(Guid cardId, Guid requesterId, UpdateCardRequest request)
    {
        var card = await RequireCardAsync(cardId);
        var access = await _boardClient.GetBoardAccessAsync(card.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to update cards.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot update cards on a closed board.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot modify cards.");

        if (request.Title is not null) card.Title = request.Title;
        if (request.Description is not null) card.Description = request.Description;
        if (request.CoverColor is not null) card.CoverColor  = request.CoverColor;

        if (request.Status is not null)
        {
            var status = request.Status.ToUpperInvariant();
            if (!ValidStatuses.Contains(status))
                throw new ArgumentException($"Invalid status '{request.Status}'. Must be one of: {string.Join(", ", ValidStatuses)}");
            card.Status = status;
        }

        if (request.Priority is not null)
        {
            var priority = request.Priority.ToUpperInvariant();
            if (!ValidPriorities.Contains(priority))
                throw new ArgumentException($"Invalid priority '{request.Priority}'. Must be one of: {string.Join(", ", ValidPriorities)}");
            card.Priority = priority;
        }

        if (request.DueDate.HasValue) card.DueDate = request.DueDate;
        if (request.StartDate.HasValue) card.StartDate = request.StartDate;

        card = await _repo.UpdateAsync(card);
        return CardResponse.FromCard(card);
    }

    // Move / Reorder 

    public async Task<CardResponse> MoveCardAsync(Guid cardId, Guid requesterId, MoveCardRequest request)
    {
        var card = await RequireCardAsync(cardId);
        var access = await _boardClient.GetBoardAccessAsync(card.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to move cards.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot move cards on a closed board.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot move cards.");

        var sourceListId = card.ListId;

        // Determine new position in target list
        if (request.TargetPosition.HasValue)
        {
            card.Position = request.TargetPosition.Value;
        }
        else
        {
            var maxPos = await _repo.FindMaxPositionByListIdAsync(request.TargetListId);
            card.Position = maxPos + 1;
        }

        card.ListId = request.TargetListId;
        card = await _repo.UpdateAsync(card);

        // Compact positions in the source list after removal
        await CompactPositionsAsync(sourceListId);

        return CardResponse.FromCard(card);
    }

    // Atomically reorders all active cards in a list.
    public async Task<IEnumerable<CardResponse>> ReorderCardsAsync(Guid requesterId, ReorderCardsRequest request)
    {
        var currentCards = (await _repo.FindByListIdOrderByPositionAsync(request.ListId)).ToList();
        if (!currentCards.Any())
            throw new InvalidOperationException("No cards found in this list.");

        var access = await _boardClient.GetBoardAccessAsync(currentCards.First().BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to reorder cards.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot reorder cards on a closed board.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot reorder cards.");

        var currentIds = currentCards.Select(c => c.CardId).ToHashSet();
        var requestedIds = request.OrderedCardIds.ToHashSet();

        if (!currentIds.SetEquals(requestedIds))
            throw new InvalidOperationException("The reorder request must include all active cards in the list — " +
                "no extras and no missing cards.");

        var cardLookup = currentCards.ToDictionary(c => c.CardId);

        var cardsToUpdate = request.OrderedCardIds
            .Select((id, index) =>
            {
                var c = cardLookup[id];
                c.Position = index + 1;
                return c;
            })
            .ToList();

        await _repo.BatchUpdatePositionsAsync(cardsToUpdate);

        return cardsToUpdate
            .OrderBy(c => c.Position)
            .Select(CardResponse.FromCard);
    }

    // Archive / Unarchive 

    public async Task<CardResponse> ArchiveCardAsync(Guid cardId, Guid requesterId)
    {
        var card = await RequireCardAsync(cardId);
        var access = await _boardClient.GetBoardAccessAsync(card.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to archive cards.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot archive cards.");

        if (card.IsArchived)
            throw new InvalidOperationException("Card is already archived.");

        card.IsArchived = true;
        card = await _repo.UpdateAsync(card);

        // Compact positions of remaining active cards in the list
        await CompactPositionsAsync(card.ListId);

        return CardResponse.FromCard(card);
    }

    public async Task<CardResponse> UnarchiveCardAsync(Guid cardId, Guid requesterId)
    {
        var card = await _repo.FindByCardIdIncludingArchivedAsync(cardId)
            ?? throw new KeyNotFoundException($"Card {cardId} not found.");

        if (!card.IsArchived)
            throw new InvalidOperationException("Card is not archived.");

        var access = await _boardClient.GetBoardAccessAsync(card.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to unarchive cards.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot unarchive cards.");

        card.IsArchived = false;

        // Append to the end of the list
        var maxPosition = await _repo.FindMaxPositionByListIdAsync(card.ListId);
        card.Position = maxPosition + 1;

        card = await _repo.UpdateAsync(card);
        return CardResponse.FromCard(card);
    }

    // Hard delete 

    public async Task DeleteCardAsync(Guid cardId, Guid requesterId)
    {
        var card = await _repo.FindByCardIdIncludingArchivedAsync(cardId)
            ?? throw new KeyNotFoundException($"Card {cardId} not found.");

        var access = await _boardClient.GetBoardAccessAsync(card.BoardId, requesterId);

        if (!access.IsAdminOrCreator)
            throw new UnauthorizedAccessException("Only board Admins or the board creator can permanently delete cards.");

        if (!card.IsArchived)
            throw new InvalidOperationException("Only archived cards can be permanently deleted. Archive the card first.");

        await _repo.DeleteByCardIdAsync(cardId);
    }

    // Assignment & Priority 

    public async Task<CardResponse> SetAssigneeAsync(Guid cardId, Guid requesterId, SetAssigneeRequest request)
    {
        var card = await RequireCardAsync(cardId);
        var access = await _boardClient.GetBoardAccessAsync(card.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to assign cards.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot modify cards on a closed board.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot assign cards.");

        card.AssigneeId = request.AssigneeId;
        card = await _repo.UpdateAsync(card);
        return CardResponse.FromCard(card);
    }

    public async Task<CardResponse> SetPriorityAsync(Guid cardId, Guid requesterId, SetPriorityRequest request)
    {
        var card = await RequireCardAsync(cardId);
        var access = await _boardClient.GetBoardAccessAsync(card.BoardId, requesterId);

        if (!access.IsMember)
            throw new UnauthorizedAccessException("You must be a board member to change card priority.");

        if (access.IsClosed)
            throw new InvalidOperationException("Cannot modify cards on a closed board.");

        if (access.IsObserver)
            throw new UnauthorizedAccessException("Observers cannot change card priority.");

        var priority = request.Priority.ToUpperInvariant();
        if (!ValidPriorities.Contains(priority))
            throw new ArgumentException($"Invalid priority '{request.Priority}'. Must be one of: {string.Join(", ", ValidPriorities)}");

        card.Priority = priority;
        card = await _repo.UpdateAsync(card);
        return CardResponse.FromCard(card);
    }

    // Overdue detection 

    public async Task<IEnumerable<CardResponse>> GetOverdueCardsAsync(Guid requesterId)
    {
        var overdueCards = await _repo.FindByDueDateBeforeAsync(DateTime.UtcNow);
        return overdueCards.Select(CardResponse.FromCard);
    }

    // Private helpers 

    private async Task<Card> RequireCardAsync(Guid cardId) =>
        await _repo.FindByCardIdAsync(cardId)
            ?? throw new KeyNotFoundException($"Card {cardId} not found.");

    private async Task CompactPositionsAsync(Guid listId)
    {
        var activeCards = (await _repo.FindByListIdOrderByPositionAsync(listId)).ToList();
        for (int i = 0; i < activeCards.Count; i++)
            activeCards[i].Position = i + 1;

        if (activeCards.Any())
            await _repo.BatchUpdatePositionsAsync(activeCards);
    }
}
