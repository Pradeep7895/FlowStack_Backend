using FlowStack.BoardService.DTOs;

namespace FlowStack.BoardService.Services;

public interface IBoardService
{
    // Board CRUD 
    Task<BoardDetailResponse> CreateBoardAsync(Guid requesterId, CreateBoardRequest request);
    Task<BoardDetailResponse> GetBoardByIdAsync(Guid boardId, Guid requesterId);
    Task<IEnumerable<BoardResponse>> GetBoardsByWorkspaceAsync(Guid workspaceId, Guid requesterId);
    Task<IEnumerable<BoardResponse>> GetBoardsByMemberAsync(Guid userId);
    Task<IEnumerable<BoardResponse>> GetBoardsByCreatorAsync(Guid userId);
    Task<BoardDetailResponse> UpdateBoardAsync(Guid boardId, Guid requesterId, UpdateBoardRequest request);
    Task<BoardDetailResponse> CloseBoardAsync(Guid boardId, Guid requesterId);
    Task<BoardDetailResponse> ReopenBoardAsync(Guid boardId, Guid requesterId);
    Task DeleteBoardAsync(Guid boardId, Guid requesterId);

    // Member management 
    Task<BoardMemberResponse> AddMemberAsync(Guid boardId, Guid requesterId, AddBoardMemberRequest request);
    Task RemoveMemberAsync(Guid boardId, Guid requesterId, Guid targetUserId);
    Task<BoardMemberResponse> UpdateMemberRoleAsync(Guid boardId, Guid requesterId, Guid targetUserId, UpdateBoardMemberRoleRequest request);
    Task<IEnumerable<BoardMemberResponse>> GetMembersAsync(Guid boardId, Guid requesterId);
    
    Task<BoardAccessResponse> GetBoardAccessAsync(Guid boardId, Guid userId);
}