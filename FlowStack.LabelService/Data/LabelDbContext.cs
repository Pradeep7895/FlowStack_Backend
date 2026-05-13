using FlowStack.LabelService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.LabelService.Data;

public class LabelDbContext : DbContext
{
    public LabelDbContext(DbContextOptions<LabelDbContext> options) : base(options) { }

    public DbSet<Label> Labels => Set<Label>();
    public DbSet<CardLabel> CardLabels => Set<CardLabel>();
    public DbSet<Checklist> Checklists => Set<Checklist>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CardLabel composite key and indexes
        modelBuilder.Entity<CardLabel>()
            .HasKey(cl => new { cl.CardId, cl.LabelId });

        modelBuilder.Entity<CardLabel>()
            .HasOne(cl => cl.Label)
            .WithMany()
            .HasForeignKey(cl => cl.LabelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CardLabel>()
            .HasIndex(cl => cl.CardId);

        // Checklist relations
        modelBuilder.Entity<Checklist>()
            .HasMany(c => c.Items)
            .WithOne(i => i.Checklist)
            .HasForeignKey(i => i.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Checklist>()
            .HasIndex(c => c.CardId);

        modelBuilder.Entity<Label>()
            .HasIndex(l => l.BoardId);
    }

}
