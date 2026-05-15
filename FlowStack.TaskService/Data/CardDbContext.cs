using FlowStack.TaskService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.TaskService.Data;

public class CardDbContext : DbContext
{
    public CardDbContext(DbContextOptions<CardDbContext> options) : base(options) { }

    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("task");
        
        // Global filter: exclude archived cards by default
        modelBuilder.Entity<Card>().HasQueryFilter(c => !c.IsArchived);

        base.OnModelCreating(modelBuilder);
    }

}
