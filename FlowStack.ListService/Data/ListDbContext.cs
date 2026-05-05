using FlowStack.ListService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.ListService.Data;

public class ListDbContext : DbContext
{
    public ListDbContext(DbContextOptions<ListDbContext> options) : base(options) { }

    public DbSet<TaskList> TaskLists => Set<TaskList>();
}