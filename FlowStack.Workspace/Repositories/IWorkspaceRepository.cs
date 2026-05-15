using FlowStack.Workspace.Models;

namespace FlowStack.Workspace.Repositories;

public interface IWorkspaceRepository
{
    //  Workspace queries 
    Task<Models.Workspace?> FindByWorkspaceIdAsync(Guid workspaceId);
    Task<Models.Workspace?> FindByWorkspaceIdWithMembersAsync(Guid workspaceId);
    Task<IEnumerable<Models.Workspace>> FindByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<Models.Workspace>> FindByMemberUserIdAsync(Guid userId);
    Task<IEnumerable<Models.Workspace>> FindByVisibilityAsync(WorkspaceVisibility visibility);
    Task<IEnumerable<Models.Workspace>> FindAllAsync();
    Task<IEnumerable<Models.Workspace>> FindPublicWorkspacesAsync();
    Task<bool> ExistsByNameAndOwnerIdAsync(string name, Guid ownerId);
    Task<int> CountByOwnerIdAsync(Guid ownerId);

    Task<Models.Workspace> CreateAsync(Models.Workspace workspace);
    Task<Models.Workspace> UpdateAsync(Models.Workspace workspace);
    Task DeleteAsync(Guid workspaceId);

    //  WorkspaceMember queries 
    Task<WorkspaceMember?> FindMemberAsync(Guid workspaceId, Guid userId);
    Task<WorkspaceMember?> FindMemberByIdAsync(Guid memberId);
    Task<IEnumerable<WorkspaceMember>> GetMembersAsync(Guid workspaceId);
    Task<bool> IsMemberAsync(Guid workspaceId, Guid userId);
    Task<bool> IsAdminOrOwnerAsync(Guid workspaceId, Guid userId, Guid ownerId);

    Task<WorkspaceMember> AddMemberAsync(WorkspaceMember member);
    Task<WorkspaceMember> UpdateMemberAsync(WorkspaceMember member);
    Task RemoveMemberAsync(Guid workspaceId, Guid userId);
    Task RemoveAllMembersAsync(Guid workspaceId);
}