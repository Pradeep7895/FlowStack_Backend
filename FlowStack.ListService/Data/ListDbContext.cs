using FlowStack.ListService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.ListService.Data;

public class ListDbContext : DbContext
{
    public ListDbContext(DbContextOptions<ListDbContext> options) : base(options) { }

    public DbSet<TaskList> TaskLists => Set<TaskList>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("list");

        // Global filter: exclude archived lists by default
        modelBuilder.Entity<TaskList>().HasQueryFilter(l => !l.IsArchived);

        base.OnModelCreating(modelBuilder);
    }
}