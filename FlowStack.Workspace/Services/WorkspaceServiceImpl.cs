using FlowStack.Workspace.DTOs;
using FlowStack.Workspace.Models;
using FlowStack.Workspace.Repositories;

namespace FlowStack.Workspace.Services;

public class WorkspaceServiceImpl : IWorkspaceService
{
    private readonly IWorkspaceRepository _repo;

    public WorkspaceServiceImpl(IWorkspaceRepository repo)
    {
        _repo = repo;
    }

    // Workspace CRUD 

    public async Task<WorkspaceDetailResponse> CreateWorkspaceAsync(
        Guid requesterId, CreateWorkspaceRequest request)
    {
        if (await _repo.ExistsByNameAndOwnerIdAsync(request.Name, requesterId))
            throw new InvalidOperationException(
                $"You already have a workspace named '{request.Name}'.");

        var workspace = new Models.Workspace
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = requesterId,
            Visibility  = request.Visibility,
            LogoUrl = request.LogoUrl
        };

        workspace = await _repo.CreateAsync(workspace);

        // Auto-enroll creator as Admin member
        var ownerMember = new WorkspaceMember
        {
            WorkspaceId = workspace.WorkspaceId,
            UserId = requesterId,
            Role = WorkspaceMemberRole.Admin
        };
        await _repo.AddMemberAsync(ownerMember);

        // Reload with members so MemberCount is accurate
        var full = await _repo.FindByWorkspaceIdWithMembersAsync(workspace.WorkspaceId);
        return WorkspaceDetailResponse.FromWorkspaceWithMembers(full!);
    }

    public async Task<WorkspaceDetailResponse> GetByIdAsync(Guid workspaceId, Guid requesterId)
    {
        var workspace = await RequireWorkspaceWithMembersAsync(workspaceId);

        // Private workspaces are only visible to members
        if (workspace.Visibility == WorkspaceVisibility.Private &&
            !await _repo.IsMemberAsync(workspaceId, requesterId) &&
            workspace.OwnerId != requesterId)
        {
            throw new UnauthorizedAccessException(
                "You do not have access to this private workspace.");
        }

        return WorkspaceDetailResponse.FromWorkspaceWithMembers(workspace);
    }

    public async Task<IEnumerable<WorkspaceResponse>> GetByOwnerAsync(Guid ownerId)
    {
        var workspaces = await _repo.FindByOwnerIdAsync(ownerId);
        return workspaces.Select(WorkspaceResponse.FromWorkspace);
    }

    public async Task<IEnumerable<WorkspaceResponse>> GetByMemberAsync(Guid userId)
    {
        var workspaces = await _repo.FindByMemberUserIdAsync(userId);
        return workspaces.Select(WorkspaceResponse.FromWorkspace);
    }

    public async Task<IEnumerable<WorkspaceResponse>> GetAllAsync()
    {
        var workspaces = await _repo.FindAllAsync();
        return workspaces.Select(WorkspaceResponse.FromWorkspace);
    }

    public async Task<IEnumerable<WorkspaceResponse>> GetPublicWorkspacesAsync()
    {
        var workspaces = await _repo.FindPublicWorkspacesAsync();
        return workspaces.Select(WorkspaceResponse.FromWorkspace);
    }

    public async Task<WorkspaceDetailResponse> UpdateWorkspaceAsync(
        Guid workspaceId, Guid requesterId, UpdateWorkspaceRequest request)
    {
        var workspace = await RequireWorkspaceAsync(workspaceId);
        await RequireAdminOrOwnerAsync(workspaceId, requesterId, workspace.OwnerId);

        // Patch only supplied fields
        if (request.Name is not null)
        {
            if (request.Name != workspace.Name &&
                await _repo.ExistsByNameAndOwnerIdAsync(request.Name, workspace.OwnerId))
                throw new InvalidOperationException(
                    $"You already have a workspace named '{request.Name}'.");
            workspace.Name = request.Name;
        }

        if (request.Description is not null) workspace.Description = request.Description;
        if (request.LogoUrl     is not null) workspace.LogoUrl     = request.LogoUrl;
        if (request.Visibility  is not null) workspace.Visibility  = request.Visibility.Value;

        workspace = await _repo.UpdateAsync(workspace);

        var full = await _repo.FindByWorkspaceIdWithMembersAsync(workspace.WorkspaceId);
        return WorkspaceDetailResponse.FromWorkspaceWithMembers(full!);
    }

    public async Task DeleteWorkspaceAsync(Guid workspaceId, Guid requesterId)
    {
        var workspace = await RequireWorkspaceAsync(workspaceId);

        // Only the workspace owner can permanently delete it
        if (workspace.OwnerId != requesterId)
            throw new UnauthorizedAccessException(
                "Only the workspace owner can delete this workspace.");

        await _repo.DeleteAsync(workspaceId);
    }

    // Member management 

    public async Task<WorkspaceMemberResponse> AddMemberAsync(Guid workspaceId, Guid requesterId, AddMemberRequest request)
    {
        var workspace = await RequireWorkspaceAsync(workspaceId);
        await RequireAdminOrOwnerAsync(workspaceId, requesterId, workspace.OwnerId);

        // Prevent duplicate membership
        if (await _repo.IsMemberAsync(workspaceId, request.UserId))
            throw new InvalidOperationException("User is already a member of this workspace.");

        // Prevent adding the owner as a plain member (owner is already Admin by default)
        if (request.UserId == workspace.OwnerId)
            throw new InvalidOperationException(
                "The workspace owner is already enrolled as an Admin member.");

        var member = new WorkspaceMember
        {
            WorkspaceId = workspaceId,
            UserId = request.UserId,
            Role = request.Role
        };

        member = await _repo.AddMemberAsync(member);
        return WorkspaceMemberResponse.FromMember(member);
    }

    public async Task RemoveMemberAsync(Guid workspaceId, Guid requesterId, Guid targetUserId)
    {
        var workspace = await RequireWorkspaceAsync(workspaceId);

        // A member can remove themselves; an Admin/Owner can remove anyone
        bool isSelf = requesterId == targetUserId;
        bool isAdminOrOwner  = await _repo.IsAdminOrOwnerAsync(
                                    workspaceId, requesterId, workspace.OwnerId);

        if (!isSelf && !isAdminOrOwner)
            throw new UnauthorizedAccessException(
                "You do not have permission to remove this member.");

        // Owner cannot be removed from their own workspace
        if (targetUserId == workspace.OwnerId)
            throw new InvalidOperationException(
                "The workspace owner cannot be removed. Transfer ownership first.");

        if (!await _repo.IsMemberAsync(workspaceId, targetUserId))
            throw new KeyNotFoundException("Member not found in this workspace.");

        await _repo.RemoveMemberAsync(workspaceId, targetUserId);
    }

    public async Task<WorkspaceMemberResponse> UpdateMemberRoleAsync(
        Guid workspaceId, Guid requesterId, Guid targetUserId, UpdateMemberRoleRequest request)
    {
        var workspace = await RequireWorkspaceAsync(workspaceId);
        await RequireAdminOrOwnerAsync(workspaceId, requesterId, workspace.OwnerId);

        var member = await _repo.FindMemberAsync(workspaceId, targetUserId)
            ?? throw new KeyNotFoundException("Member not found in this workspace.");

        // Prevent demoting the owner's admin role via this endpoint
        if (targetUserId == workspace.OwnerId)
            throw new InvalidOperationException(
                "The workspace owner's role cannot be changed.");

        member.Role = request.Role;
        member = await _repo.UpdateMemberAsync(member);
        return WorkspaceMemberResponse.FromMember(member);
    }

    public async Task<IEnumerable<WorkspaceMemberResponse>> GetMembersAsync(Guid workspaceId, Guid requesterId)
    {
        var workspace = await RequireWorkspaceAsync(workspaceId);

        // Private workspace member list is only visible to existing members
        if (workspace.Visibility == WorkspaceVisibility.Private &&
            !await _repo.IsMemberAsync(workspaceId, requesterId) &&
            workspace.OwnerId != requesterId)
        {
            throw new UnauthorizedAccessException(
                "You must be a member to view the member list of a private workspace.");
        }

        var members = await _repo.GetMembersAsync(workspaceId);
        return members.Select(WorkspaceMemberResponse.FromMember);
    }

    // Membership checks 

    public async Task<bool> IsMemberAsync(Guid workspaceId, Guid userId) =>
        await _repo.IsMemberAsync(workspaceId, userId);

    public async Task<bool> IsAdminOrOwnerAsync(Guid workspaceId, Guid userId)
    {
        var workspace = await _repo.FindByWorkspaceIdAsync(workspaceId);
        if (workspace is null) return false;
        return await _repo.IsAdminOrOwnerAsync(workspaceId, userId, workspace.OwnerId);
    }

    // Private helpers 

    private async Task<Models.Workspace> RequireWorkspaceAsync(Guid workspaceId) =>
        await _repo.FindByWorkspaceIdAsync(workspaceId)
            ?? throw new KeyNotFoundException($"Workspace {workspaceId} not found.");

    private async Task<Models.Workspace> RequireWorkspaceWithMembersAsync(Guid workspaceId) =>
        await _repo.FindByWorkspaceIdWithMembersAsync(workspaceId)
            ?? throw new KeyNotFoundException($"Workspace {workspaceId} not found.");

    private async Task RequireAdminOrOwnerAsync(Guid workspaceId, Guid requesterId, Guid ownerId)
    {
        if (!await _repo.IsAdminOrOwnerAsync(workspaceId, requesterId, ownerId))
            throw new UnauthorizedAccessException(
                "Only workspace Admins or the Owner can perform this action.");
    }
}