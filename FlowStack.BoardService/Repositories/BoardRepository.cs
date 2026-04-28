using FlowStack.BoardService.Data;
using FlowStack.BoardService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.BoardService.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly BoardDbContext _db;

    public BoardRepository(BoardDbContext db)
    {
        _db = db;
    }

    // Board queries 

    public async Task<Board?> FindByBoardIdAsync(Guid boardId) =>
        await _db.Boards.FirstOrDefaultAsync(b => b.BoardId == boardId);

    public async Task<Board?> FindByBoardIdWithMembersAsync(Guid boardId) =>
        await _db.Boards
                .Include(b => b.Members)
                .FirstOrDefaultAsync(b => b.BoardId == boardId);

    public async Task<IEnumerable<Board>> FindByWorkspaceIdAsync(Guid workspaceId) =>
        await _db.Boards
                .Where(b => b.WorkspaceId == workspaceId)
                .Include(b => b.Members)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

    public async Task<IEnumerable<Board>> FindByCreatedByIdAsync(Guid userId) =>
        await _db.Boards
                .Where(b => b.CreatedById == userId)
                .Include(b => b.Members)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

    public async Task<IEnumerable<Board>> FindByMemberUserIdAsync(Guid userId) =>
        await _db.Boards
                .Include(b => b.Members)
                .Where(b => b.Members.Any(m => m.UserId == userId))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

    public async Task<IEnumerable<Board>> FindByVisibilityAsync(BoardVisibility visibility) =>
        await _db.Boards
                .Where(b => b.Visibility == visibility)
                .Include(b => b.Members)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

    public async Task<IEnumerable<Board>> FindByIsClosedAsync(bool isClosed) =>
        await _db.Boards
                .Where(b => b.IsClosed == isClosed)
                .Include(b => b.Members)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

    public async Task<IEnumerable<Board>> FindByWorkspaceIdAndIsClosedAsync(
        Guid workspaceId, bool isClosed) =>
        await _db.Boards
                .Where(b => b.WorkspaceId == workspaceId && b.IsClosed == isClosed)
                .Include(b => b.Members)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

    public async Task<int> CountByWorkspaceIdAsync(Guid workspaceId) =>
        await _db.Boards.CountAsync(b => b.WorkspaceId == workspaceId);

    public async Task<bool> ExistsByNameAndWorkspaceIdAsync(string name, Guid workspaceId) =>
        await _db.Boards.AnyAsync(b =>
            b.Name.ToLower() == name.ToLower() && b.WorkspaceId == workspaceId);

    public async Task<Board> CreateAsync(Board board)
    {
        _db.Boards.Add(board);
        await _db.SaveChangesAsync();
        return board;
    }

    public async Task<Board> UpdateAsync(Board board)
    {
        board.UpdatedAt = DateTime.UtcNow;
        _db.Boards.Update(board);
        await _db.SaveChangesAsync();
        return board;
    }

    public async Task DeleteAsync(Guid boardId)
    {
        var board = await FindByBoardIdAsync(boardId);
        if (board is not null)
        {
            _db.Boards.Remove(board);
            await _db.SaveChangesAsync();
        }
    }

    // BoardMember queries 

    public async Task<BoardMember?> FindMemberAsync(Guid boardId, Guid userId) =>
        await _db.BoardMembers
                .FirstOrDefaultAsync(m => m.BoardId == boardId && m.UserId == userId);

    public async Task<BoardMember?> FindMemberByIdAsync(Guid boardMemberId) =>
        await _db.BoardMembers
                .FirstOrDefaultAsync(m => m.BoardMemberId == boardMemberId);

    public async Task<IEnumerable<BoardMember>> GetMembersAsync(Guid boardId) =>
        await _db.BoardMembers
                .Where(m => m.BoardId == boardId)
                .OrderBy(m => m.AddedAt)
                .ToListAsync();

    public async Task<bool> IsMemberAsync(Guid boardId, Guid userId) =>
        await _db.BoardMembers
                .AnyAsync(m => m.BoardId == boardId && m.UserId == userId);

    public async Task<bool> IsAdminOrCreatorAsync(
        Guid boardId, Guid userId, Guid createdById)
    {
        if (userId == createdById) return true;
        return await _db.BoardMembers.AnyAsync(m =>
            m.BoardId == boardId &&
            m.UserId == userId  &&
            m.Role == BoardMemberRole.Admin);
    }

    public async Task<BoardMemberRole?> GetMemberRoleAsync(Guid boardId, Guid userId)
    {
        var member = await FindMemberAsync(boardId, userId);
        return member?.Role;
    }

    public async Task<BoardMember> AddMemberAsync(BoardMember member)
    {
        _db.BoardMembers.Add(member);
        await _db.SaveChangesAsync();
        return member;
    }

    public async Task<BoardMember> UpdateMemberAsync(BoardMember member)
    {
        _db.BoardMembers.Update(member);
        await _db.SaveChangesAsync();
        return member;
    }

    public async Task RemoveMemberAsync(Guid boardId, Guid userId)
    {
        var member = await FindMemberAsync(boardId, userId);
        if (member is not null)
        {
            _db.BoardMembers.Remove(member);
            await _db.SaveChangesAsync();
        }
    }
}