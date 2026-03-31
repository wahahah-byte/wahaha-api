using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class StreakRepository : Repository<Streak, int>, IStreakRepository
{
    public StreakRepository(WahahaDbContext context) : base(context) { }

    public async Task<IEnumerable<Streak>> GetByUserAsync(Guid userId)
        => await _dbSet
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CurrentCount)
            .ToListAsync();

    public async Task<IEnumerable<Streak>> GetActiveByUserAsync(Guid userId)
        => await _dbSet
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.CurrentCount)
            .ToListAsync();

    public async Task<bool> IncrementAsync(int id)
    {
        var streak = await _dbSet.FindAsync(id);
        if (streak == null) return false;

        streak.CurrentCount++;
        streak.LastActivityDate = DateTime.UtcNow;
        streak.IsActive = true;

        if (streak.CurrentCount > streak.LongestCount)
            streak.LongestCount = streak.CurrentCount;

        streak.BonusMultiplier = streak.CurrentCount switch
        {
            >= 30 => 2.0m,
            >= 14 => 1.8m,
            >= 7  => 1.5m,
            >= 3  => 1.2m,
            _     => 1.0m
        };

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetAsync(int id)
    {
        var streak = await _dbSet.FindAsync(id);
        if (streak == null) return false;

        streak.CurrentCount = 0;
        streak.BonusMultiplier = 1.0m;
        streak.IsActive = false;

        await _context.SaveChangesAsync();
        return true;
    }
}
