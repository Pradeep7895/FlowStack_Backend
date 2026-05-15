using FlowStack.BoardService.Models;

namespace FlowStack.BoardService.Repositories;

public interface IBoardRepository
{
    // Board queries
    Task<Board?> FindByBoardIdAsync(Guid boardId);
    Task<Board?> FindByBoardIdWithMembersAsync(Guid boardId);
    Task<IEnumerable<Board>> FindByWorkspaceIdAsync(Guid workspaceId);
    Task<IEnumerable<Board>> FindByCreatedByIdAsync(Guid userId);
    Task<IEnumerable<Board>> FindByMemberUserIdAsync(Guid userId);
    Task<IEnumerable<Board>> FindByVisibilityAsync(BoardVisibility visibility);
    Task<IEnumerable<Board>> FindByIsClosedAsync(bool isClosed);
    Task<IEnumerable<Board>> FindByWorkspaceIdAndIsClosedAsync(Guid workspaceId, bool isClosed);
    Task<int> CountByWorkspaceIdAsync(Guid workspaceId);
    Task<bool> ExistsByNameAndWorkspaceIdAsync(string name, Guid workspaceId);

    Task<Board> CreateAsync(Board board);
    Task<Board> UpdateAsync(Board board);
    Task DeleteAsync(Guid boardId);

    // BoardMember queries 
    Task<BoardMember?> FindMemberAsync(Guid boardId, Guid userId);
    Task<BoardMember?> FindMemberByIdAsync(Guid boardMemberId);
    Task<IEnumerable<BoardMember>> GetMembersAsync(Guid boardId);
    Task<bool> IsMemberAsync(Guid boardId, Guid userId);
    Task<bool> IsAdminOrCreatorAsync(Guid boardId, Guid userId, Guid createdById);
    Task<BoardMemberRole?> GetMemberRoleAsync(Guid boardId, Guid userId);

    Task<BoardMember> AddMemberAsync(BoardMember member);
    Task<BoardMember> UpdateMemberAsync(BoardMember member);
    Task RemoveMemberAsync(Guid boardId, Guid userId);
}