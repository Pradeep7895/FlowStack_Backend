using FlowStack.Workspace.Data;
using FlowStack.Workspace.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.Workspace.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly WorkspaceDbContext _db;

    public WorkspaceRepository(WorkspaceDbContext db)
    {
        _db = db;
    }

    // Workspace queries 

    public async Task<IEnumerable<Models.Workspace>> FindAllAsync() =>
        await _db.Workspaces
                .Include(w => w.Members)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

    public async Task<Models.Workspace?> FindByWorkspaceIdAsync(Guid workspaceId) =>
        await _db.Workspaces.FirstOrDefaultAsync(w => w.WorkspaceId == workspaceId);

    public async Task<Models.Workspace?> FindByWorkspaceIdWithMembersAsync(Guid workspaceId) =>
        await _db.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.WorkspaceId == workspaceId);

    public async Task<IEnumerable<Models.Workspace>> FindByOwnerIdAsync(Guid ownerId) =>
        await _db.Workspaces
                .Where(w => w.OwnerId == ownerId)
                .Include(w => w.Members)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

    public async Task<IEnumerable<Models.Workspace>> FindByMemberUserIdAsync(Guid userId) =>
        await _db.Workspaces
                .Include(w => w.Members)
                .Where(w => w.Members.Any(m => m.UserId == userId))
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

    public async Task<IEnumerable<Models.Workspace>> FindByVisibilityAsync(
        WorkspaceVisibility visibility) =>
        await _db.Workspaces
                .Where(w => w.Visibility == visibility)
                .Include(w => w.Members)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

    public async Task<IEnumerable<Models.Workspace>> FindPublicWorkspacesAsync() =>
        await _db.Workspaces
                .Where(w => w.Visibility == WorkspaceVisibility.Public)
                .Include(w => w.Members)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

    public async Task<bool> ExistsByNameAndOwnerIdAsync(string name, Guid ownerId) =>
        await _db.Workspaces.AnyAsync(w =>
            w.Name.ToLower() == name.ToLower() && w.OwnerId == ownerId);

    public async Task<int> CountByOwnerIdAsync(Guid ownerId) =>
        await _db.Workspaces.CountAsync(w => w.OwnerId == ownerId);

    public async Task<Models.Workspace> CreateAsync(Models.Workspace workspace)
    {
        _db.Workspaces.Add(workspace);
        await _db.SaveChangesAsync();
        return workspace;
    }

    public async Task<Models.Workspace> UpdateAsync(Models.Workspace workspace)
    {
        workspace.UpdatedAt = DateTime.UtcNow;
        _db.Workspaces.Update(workspace);
        await _db.SaveChangesAsync();
        return workspace;
    }

    public async Task DeleteAsync(Guid workspaceId)
    {
        var workspace = await FindByWorkspaceIdAsync(workspaceId);
        if (workspace is not null)
        {
            _db.Workspaces.Remove(workspace);
            await _db.SaveChangesAsync();
        }
    }

    //  WorkspaceMember queries 

    public async Task<WorkspaceMember?> FindMemberAsync(Guid workspaceId, Guid userId) =>
        await _db.WorkspaceMembers
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);

    public async Task<WorkspaceMember?> FindMemberByIdAsync(Guid memberId) =>
        await _db.WorkspaceMembers.FirstOrDefaultAsync(m => m.MemberId == memberId);

    public async Task<IEnumerable<WorkspaceMember>> GetMembersAsync(Guid workspaceId) =>
        await _db.WorkspaceMembers
                .Where(m => m.WorkspaceId == workspaceId)
                .OrderBy(m => m.JoinedAt)
                .ToListAsync();

    public async Task<bool> IsMemberAsync(Guid workspaceId, Guid userId) =>
        await _db.WorkspaceMembers
                .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);

    public async Task<bool> IsAdminOrOwnerAsync(Guid workspaceId, Guid userId, Guid ownerId)
    {
        if (userId == ownerId) return true;
        return await _db.WorkspaceMembers
                        .AnyAsync(m =>
                            m.WorkspaceId == workspaceId &&
                            m.UserId == userId &&
                            m.Role == WorkspaceMemberRole.Admin);
    }

    public async Task<WorkspaceMember> AddMemberAsync(WorkspaceMember member)
    {
        _db.WorkspaceMembers.Add(member);
        await _db.SaveChangesAsync();
        return member;
    }

    public async Task<WorkspaceMember> UpdateMemberAsync(WorkspaceMember member)
    {
        _db.WorkspaceMembers.Update(member);
        await _db.SaveChangesAsync();
        return member;
    }

    public async Task RemoveMemberAsync(Guid workspaceId, Guid userId)
    {
        var member = await FindMemberAsync(workspaceId, userId);
        if (member is not null)
        {
            _db.WorkspaceMembers.Remove(member);
            await _db.SaveChangesAsync();
        }
    }

    public async Task RemoveAllMembersAsync(Guid workspaceId)
    {
        var members = _db.WorkspaceMembers.Where(m => m.WorkspaceId == workspaceId);
        _db.WorkspaceMembers.RemoveRange(members);
        await _db.SaveChangesAsync();
    }
}