using FlowStack.BoardService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.BoardService.Data;

public class BoardDbContext : DbContext
{
    public BoardDbContext(DbContextOptions<BoardDbContext> options) : base(options) { }

    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardMember> BoardMembers => Set<BoardMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("board");
        base.OnModelCreating(modelBuilder);
    }

}