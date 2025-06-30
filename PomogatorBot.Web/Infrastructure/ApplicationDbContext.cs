using Microsoft.EntityFrameworkCore;
using PomogatorBot.Web.Infrastructure.Entities;

namespace PomogatorBot.Web.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<BroadcastHistory> BroadcastHistory => Set<BroadcastHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BroadcastHistory>()
            .Property(e => e.MessageEntities)
            .HasColumnType("jsonb");
    }
}
