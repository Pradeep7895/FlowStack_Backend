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

}
