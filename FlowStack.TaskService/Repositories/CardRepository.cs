using FlowStack.TaskService.Data;
using FlowStack.TaskService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.TaskService.Repositories;

public class CardRepository : ICardRepository
{
    private readonly CardDbContext _db;

    public CardRepository(CardDbContext db)
    {
        _db = db;
    }

    public async Task<Card?> FindByCardIdAsync(Guid cardId) =>
        await _db.Cards.FirstOrDefaultAsync(c => c.CardId == cardId);

    public async Task<Card?> FindByCardIdIncludingArchivedAsync(Guid cardId) =>
        await _db.Cards.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CardId == cardId);

    public async Task<IEnumerable<Card>> FindByListIdAsync(Guid listId) =>
        await _db.Cards.Where(c => c.ListId == listId).ToListAsync();

    public async Task<IEnumerable<Card>> FindByListIdOrderByPositionAsync(Guid listId) =>
        await _db.Cards.Where(c => c.ListId == listId)
                        .OrderBy(c => c.Position)
                        .ToListAsync();

    public async Task<int> CountByListIdAsync(Guid listId) =>
        await _db.Cards.CountAsync(c => c.ListId == listId);

    public async Task<int> FindMaxPositionByListIdAsync(Guid listId)
    {
        var cards = await _db.Cards.Where(c => c.ListId == listId).ToListAsync();
        return cards.Any() ? cards.Max(c => c.Position) : 0;
    }

    public async Task<IEnumerable<Card>> FindByBoardIdAsync(Guid boardId) =>
        await _db.Cards.Where(c => c.BoardId == boardId)
                        .OrderBy(c => c.ListId)
                        .ThenBy(c => c.Position)
                        .ToListAsync();

    public async Task<IEnumerable<Card>> FindByAssigneeIdAsync(Guid assigneeId) =>
        await _db.Cards.Where(c => c.AssigneeId == assigneeId)
                        .OrderByDescending(c => c.CreatedAt)
                        .ToListAsync();

    public async Task<IEnumerable<Card>> FindByStatusAsync(Guid boardId, string status) =>
        await _db.Cards.Where(c => c.BoardId == boardId && c.Status == status)
                        .OrderBy(c => c.Position)
                        .ToListAsync();

    public async Task<IEnumerable<Card>> FindByPriorityAsync(Guid boardId, string priority) =>
        await _db.Cards.Where(c => c.BoardId == boardId && c.Priority == priority)
                        .OrderBy(c => c.Position)
                        .ToListAsync();

    public async Task<IEnumerable<Card>> FindByDueDateBeforeAsync(DateTime dueDate) =>
        await _db.Cards.Where(c => c.DueDate != null && c.DueDate < dueDate && c.Status != "DONE")
                        .OrderBy(c => c.DueDate)
                        .ToListAsync();

    public async Task<IEnumerable<Card>> FindArchivedByListIdAsync(Guid listId) =>
        await _db.Cards.IgnoreQueryFilters()
                        .Where(c => c.ListId == listId && c.IsArchived)
                        .OrderBy(c => c.Position)
                        .ToListAsync();

    public async Task<IEnumerable<Card>> FindArchivedByBoardIdAsync(Guid boardId) =>
        await _db.Cards.IgnoreQueryFilters()
                        .Where(c => c.BoardId == boardId && c.IsArchived)
                        .OrderByDescending(c => c.UpdatedAt)
                        .ToListAsync();

    public async Task<Card> CreateAsync(Card card)
    {
        _db.Cards.Add(card);
        await _db.SaveChangesAsync();
        return card;
    }

    public async Task<Card> UpdateAsync(Card card)
    {
        card.UpdatedAt = DateTime.UtcNow;
        _db.Cards.Update(card);
        await _db.SaveChangesAsync();
        return card;
    }

    // Atomically batch-updates Position for all cards in the collection.
    // Uses an explicit EF Core transaction so all position updates
    // are committed together — if any update fails, all roll back.
    public async Task BatchUpdatePositionsAsync(IEnumerable<Card> cards)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var card in cards)
            {
                card.UpdatedAt = DateTime.UtcNow;
                _db.Entry(card).Property(c => c.Position).IsModified  = true;
                _db.Entry(card).Property(c => c.UpdatedAt).IsModified = true;
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

    public async Task DeleteByCardIdAsync(Guid cardId)
    {
        var card = await _db.Cards
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(c => c.CardId == cardId);
        if (card is not null)
        {
            _db.Cards.Remove(card);
            await _db.SaveChangesAsync();
        }
    }
}
