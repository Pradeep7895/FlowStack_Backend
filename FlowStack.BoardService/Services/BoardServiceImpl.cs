using FlowStack.BoardService.DTOs;
using FlowStack.BoardService.Helpers;
using FlowStack.BoardService.Models;
using FlowStack.BoardService.Repositories;

namespace FlowStack.BoardService.Services;

public class BoardServiceImpl : IBoardService
{
    private readonly IBoardRepository _repo;
    private readonly WorkspaceClient  _workspaceClient;

    public BoardServiceImpl(IBoardRepository repo, WorkspaceClient workspaceClient)
    {
        _repo = repo;
        _workspaceClient = workspaceClient;
    }

    // Board CRUD 

    public async Task<BoardDetailResponse> CreateBoardAsync(Guid requesterId, CreateBoardRequest request)
    {
        // Verify requester belongs to the workspace before allowing board creation
        var isMember = await _workspaceClient.IsMemberOfWorkspaceAsync(
            request.WorkspaceId, requesterId);

        if (!isMember)
            throw new UnauthorizedAccessException(
                "You must be a member of the workspace to create a board.");

        if (await _repo.ExistsByNameAndWorkspaceIdAsync(request.Name, request.WorkspaceId))
            throw new InvalidOperationException(
                $"A board named '{request.Name}' already exists in this workspace.");
        var board = new Board
        {
            WorkspaceId = request.WorkspaceId,
            Name = request.Name,
            Description = request.Description,
            Background = request.Background,
            Visibility = request.Visibility,
            CreatedById = requesterId
        };

        board = await _repo.CreateAsync(board);

        // Auto-enroll creator as Admin board member
        var creatorMember = new BoardMember
        {
            BoardId = board.BoardId,
            UserId  = requesterId,
            Role    = BoardMemberRole.Admin
        };
        await _repo.AddMemberAsync(creatorMember);

        var full = await _repo.FindByBoardIdWithMembersAsync(board.BoardId);
        return BoardDetailResponse.FromBoardWithMembers(full!);
    }

    public async Task<BoardDetailResponse> GetBoardByIdAsync(Guid boardId, Guid requesterId)
    {
        var board = await RequireBoardWithMembersAsync(boardId);

        // Private boards are only visible to members
        if (board.Visibility == BoardVisibility.Private &&
            !await _repo.IsMemberAsync(boardId, requesterId) &&
            board.CreatedById != requesterId)
        {
            throw new UnauthorizedAccessException(
                "You do not have access to this private board.");
        }

        return BoardDetailResponse.FromBoardWithMembers(board);
    }

    public async Task<IEnumerable<BoardResponse>> GetBoardsByWorkspaceAsync(
        Guid workspaceId, Guid requesterId)
    {
        var boards = await _repo.FindByWorkspaceIdAsync(workspaceId);

        // Filter out private boards the requester can't see
        var visible = boards.Where(b =>
            b.Visibility  == BoardVisibility.Public ||
            b.CreatedById == requesterId             ||
            b.Members.Any(m => m.UserId == requesterId));

        return visible.Select(BoardResponse.FromBoard);
    }

    public async Task<IEnumerable<BoardResponse>> GetBoardsByMemberAsync(Guid userId)
    {
        var boards = await _repo.FindByMemberUserIdAsync(userId);
        return boards.Select(BoardResponse.FromBoard);
    }

    public async Task<IEnumerable<BoardResponse>> GetBoardsByCreatorAsync(Guid userId)
    {
        var boards = await _repo.FindByCreatedByIdAsync(userId);
        return boards.Select(BoardResponse.FromBoard);
    }

    public async Task<BoardDetailResponse> UpdateBoardAsync(
        Guid boardId, Guid requesterId, UpdateBoardRequest request)
    {
        var board = await RequireBoardAsync(boardId);

        if (board.IsClosed)
            throw new InvalidOperationException(
                "Cannot update a closed board. Reopen it first.");

        await RequireAdminOrCreatorAsync(boardId, requesterId, board.CreatedById);

        if (request.Name is not null)
        {
            if (request.Name != board.Name &&
                await _repo.ExistsByNameAndWorkspaceIdAsync(request.Name, board.WorkspaceId))
                throw new InvalidOperationException(
                    $"A board named '{request.Name}' already exists in this workspace.");
            board.Name = request.Name;
        }

        if (request.Description is not null) board.Description = request.Description;
        if (request.Background  is not null) board.Background  = request.Background;
        if (request.Visibility  is not null) board.Visibility  = request.Visibility.Value;

        board = await _repo.UpdateAsync(board);
        var full = await _repo.FindByBoardIdWithMembersAsync(board.BoardId);
        return BoardDetailResponse.FromBoardWithMembers(full!);
    }

    public async Task<BoardDetailResponse> CloseBoardAsync(Guid boardId, Guid requesterId)
    {
        var board = await RequireBoardAsync(boardId);

        if (board.IsClosed)
            throw new InvalidOperationException("Board is already closed.");

        await RequireAdminOrCreatorAsync(boardId, requesterId, board.CreatedById);

        board.IsClosed = true;
        board = await _repo.UpdateAsync(board);
        var full = await _repo.FindByBoardIdWithMembersAsync(board.BoardId);
        return BoardDetailResponse.FromBoardWithMembers(full!);
    }

    public async Task<BoardDetailResponse> ReopenBoardAsync(Guid boardId, Guid requesterId)
    {
        var board = await RequireBoardAsync(boardId);

        if (!board.IsClosed)
            throw new InvalidOperationException("Board is already open.");

        await RequireAdminOrCreatorAsync(boardId, requesterId, board.CreatedById);

        board.IsClosed = false;
        board = await _repo.UpdateAsync(board);
        var full = await _repo.FindByBoardIdWithMembersAsync(board.BoardId);
        return BoardDetailResponse.FromBoardWithMembers(full!);
    }

    public async Task DeleteBoardAsync(Guid boardId, Guid requesterId)
    {
        var board = await RequireBoardAsync(boardId);

        // Only the board creator can permanently delete it
        if (board.CreatedById != requesterId)
            throw new UnauthorizedAccessException(
                "Only the board creator can delete this board.");

        await _repo.DeleteAsync(boardId);
    }

    // Member management 

    public async Task<BoardMemberResponse> AddMemberAsync(
        Guid boardId, Guid requesterId, AddBoardMemberRequest request)
    {
        var board = await RequireBoardAsync(boardId);

        if (board.IsClosed)
            throw new InvalidOperationException("Cannot add members to a closed board.");

        await RequireAdminOrCreatorAsync(boardId, requesterId, board.CreatedById);

        if (await _repo.IsMemberAsync(boardId, request.UserId))
            throw new InvalidOperationException("User is already a member of this board.");

        // Verify the new member belongs to the parent workspace
        var isWorkspaceMember = await _workspaceClient.IsMemberOfWorkspaceAsync(
            board.WorkspaceId, request.UserId);

        if (!isWorkspaceMember)
            throw new InvalidOperationException(
                "User must be a member of the workspace before being added to a board.");

        var member = new BoardMember
        {
            BoardId = boardId,
            UserId  = request.UserId,
            Role    = request.Role
        };

        member = await _repo.AddMemberAsync(member);
        return BoardMemberResponse.FromMember(member);
    }

    public async Task RemoveMemberAsync(
        Guid boardId, Guid requesterId, Guid targetUserId)
    {
        var board = await RequireBoardAsync(boardId);

        bool isSelf          = requesterId == targetUserId;
        bool isAdminOrCreator = await _repo.IsAdminOrCreatorAsync(
            boardId, requesterId, board.CreatedById);

        if (!isSelf && !isAdminOrCreator)
            throw new UnauthorizedAccessException(
                "You do not have permission to remove this member.");

        // Creator cannot be removed from their own board
        if (targetUserId == board.CreatedById)
            throw new InvalidOperationException(
                "The board creator cannot be removed.");

        if (!await _repo.IsMemberAsync(boardId, targetUserId))
            throw new KeyNotFoundException("Member not found on this board.");

        await _repo.RemoveMemberAsync(boardId, targetUserId);
    }

    public async Task<BoardMemberResponse> UpdateMemberRoleAsync(
        Guid boardId, Guid requesterId, Guid targetUserId,
        UpdateBoardMemberRoleRequest request)
    {
        var board = await RequireBoardAsync(boardId);
        await RequireAdminOrCreatorAsync(boardId, requesterId, board.CreatedById);

        // Creator's Admin role cannot be changed via this endpoint
        if (targetUserId == board.CreatedById)
            throw new InvalidOperationException(
                "The board creator's role cannot be changed.");

        var member = await _repo.FindMemberAsync(boardId, targetUserId)
            ?? throw new KeyNotFoundException("Member not found on this board.");

        member.Role = request.Role;
        member      = await _repo.UpdateMemberAsync(member);
        return BoardMemberResponse.FromMember(member);
    }

    public async Task<IEnumerable<BoardMemberResponse>> GetMembersAsync(
        Guid boardId, Guid requesterId)
    {
        var board = await RequireBoardAsync(boardId);

        if (board.Visibility == BoardVisibility.Private &&
            !await _repo.IsMemberAsync(boardId, requesterId) &&
            board.CreatedById != requesterId)
        {
            throw new UnauthorizedAccessException(
                "You must be a board member to view the member list.");
        }

        var members = await _repo.GetMembersAsync(boardId);
        return members.Select(BoardMemberResponse.FromMember);
    }

    // Access check 

    public async Task<BoardAccessResponse> GetBoardAccessAsync(Guid boardId, Guid userId)
    {
        var board = await _repo.FindByBoardIdWithMembersAsync(boardId);
        if (board is null)
            return new BoardAccessResponse(); // all false — board doesn't exist

        var isMember       = await _repo.IsMemberAsync(boardId, userId);
        var isAdminOrCreator = await _repo.IsAdminOrCreatorAsync(boardId, userId, board.CreatedById);
        var role           = await _repo.GetMemberRoleAsync(boardId, userId);

        return new BoardAccessResponse
        {
            IsMember         = isMember,
            IsAdminOrCreator = isAdminOrCreator,
            IsObserver       = role == BoardMemberRole.Observer,
            IsClosed         = board.IsClosed,
            WorkspaceId      = board.WorkspaceId
        };
    }

    // Private helpers 

    private async Task<Board> RequireBoardAsync(Guid boardId) =>
        await _repo.FindByBoardIdAsync(boardId)
            ?? throw new KeyNotFoundException($"Board {boardId} not found.");

    private async Task<Board> RequireBoardWithMembersAsync(Guid boardId) =>
        await _repo.FindByBoardIdWithMembersAsync(boardId)
            ?? throw new KeyNotFoundException($"Board {boardId} not found.");

    private async Task RequireAdminOrCreatorAsync(
        Guid boardId, Guid requesterId, Guid createdById)
    {
        if (!await _repo.IsAdminOrCreatorAsync(boardId, requesterId, createdById))
            throw new UnauthorizedAccessException(
                "Only board Admins or the board creator can perform this action.");
    }
}