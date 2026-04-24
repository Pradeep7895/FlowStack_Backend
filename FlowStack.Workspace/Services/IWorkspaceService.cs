using FlowStack.Workspace.DTOs;
using FlowStack.Workspace.Models;

namespace FlowStack.Workspace.Services;
public interface IWorkspaceService
{
    //  Workspace CRUD 
    Task<WorkspaceDetailResponse> CreateWorkspaceAsync(Guid requesterId, CreateWorkspaceRequest request);
    Task<WorkspaceDetailResponse> GetByIdAsync(Guid workspaceId, Guid requesterId);
    Task<IEnumerable<WorkspaceResponse>> GetByOwnerAsync(Guid ownerId);
    Task<IEnumerable<WorkspaceResponse>> GetByMemberAsync(Guid userId);
    Task<IEnumerable<WorkspaceResponse>> GetPublicWorkspacesAsync();
    Task<WorkspaceDetailResponse> UpdateWorkspaceAsync(Guid workspaceId, Guid requesterId, UpdateWorkspaceRequest request);
    Task DeleteWorkspaceAsync(Guid workspaceId, Guid requesterId);

    //  Member management 
    Task<WorkspaceMemberResponse> AddMemberAsync(Guid workspaceId, Guid requesterId, AddMemberRequest request);
    Task RemoveMemberAsync(Guid workspaceId, Guid requesterId, Guid targetUserId);
    Task<WorkspaceMemberResponse> UpdateMemberRoleAsync(Guid workspaceId, Guid requesterId, Guid targetUserId, UpdateMemberRoleRequest request);
    Task<IEnumerable<WorkspaceMemberResponse>> GetMembersAsync(Guid workspaceId, Guid requesterId);

    //  Membership checks 
    Task<bool> IsMemberAsync(Guid workspaceId, Guid userId);
    Task<bool> IsAdminOrOwnerAsync(Guid workspaceId, Guid userId);
}