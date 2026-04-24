using FlowStack.Workspace.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.Workspace.Data;

public class WorkspaceDbContext : DbContext
{
    public WorkspaceDbContext(DbContextOptions<WorkspaceDbContext> options) : base(options) { }

    public DbSet<Models.Workspace> Workspaces => Set<Models.Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

}