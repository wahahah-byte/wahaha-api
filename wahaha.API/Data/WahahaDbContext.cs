using Microsoft.EntityFrameworkCore;
using wahaha.API.Models.Domain;

namespace wahaha.API.Data;

public class WahahaDbContext: DbContext
{
    public WahahaDbContext(DbContextOptions options): base(options)
    {
            
    }

    public DbSet<AvatarItem> AvatarItems { get; set; }
    public DbSet<Minigame> Minigames { get; set; }
    public DbSet<MinigameSession> MinigameSessions { get; set; }
    public DbSet<PointTransaction> PointTransactions { get; set; }
    public DbSet<Streak> Streaks { get; set; }
    public DbSet<Models.Domain.Task> Tasks { get; set; }
    public DbSet<UserInventory> UserInventories { get; set; }
    public DbSet<Users> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Domain.Task>()
            .Property(t => t.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<Models.Domain.Task>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<AvatarItem>()
           .Property(t => t.Rarity)
           .HasConversion<string>();

        modelBuilder.Entity<AvatarItem>()
            .Property(t => t.Slot)
            .HasConversion<string>();
    }
}
