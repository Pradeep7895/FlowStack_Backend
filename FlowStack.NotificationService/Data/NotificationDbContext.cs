using FlowStack.NotificationService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.NotificationService.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notification");
        base.OnModelCreating(modelBuilder);
    }

}
