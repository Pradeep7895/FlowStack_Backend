using FlowStack.TaskService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.TaskService.Data;

public class CardDbContext : DbContext
{
    public CardDbContext(DbContextOptions<CardDbContext> options) : base(options) { }

    public DbSet<Card> Cards => Set<Card>();

}
