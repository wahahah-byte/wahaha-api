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

        base.OnModelCreating(modelBuilder);
        SeedData.Seed(modelBuilder);

        modelBuilder.Entity<Models.Domain.Task>()
            .Property(t => t.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<Models.Domain.Task>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<PointTransaction>()
            .Property(pt => pt.Type)
            .HasConversion<string>();

        modelBuilder.Entity<PointTransaction>()
            .Property(pt => pt.SourceType)
            .HasConversion<string>();

        modelBuilder.Entity<AvatarItem>()
            .Property(a => a.Slot)
            .HasConversion<string>();

        modelBuilder.Entity<AvatarItem>()
            .Property(a => a.Rarity)
            .HasConversion<string>();

        modelBuilder.Entity<Minigame>()
            .Property(m => m.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Minigame>()
            .Property(m => m.Difficulty)
            .HasConversion<string>();

        modelBuilder.Entity<MinigameSession>()
            .Property(ms => ms.Outcome)
            .HasConversion<string>();
    }
}
