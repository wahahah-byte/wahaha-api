using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class StreakRepository : Repository<Streak, int>, IStreakRepository
{
    public StreakRepository(WahahaDbContext context, ILogger<StreakRepository> logger)
        : base(context, logger) { }

    public async Task<IEnumerable<Streak>> GetByUserAsync(Guid userId)
    {
        _logger.LogDebug("Fetching streaks for user {UserId}", userId);
        return await _dbSet
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CurrentCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<Streak>> GetActiveByUserAsync(Guid userId)
    {
        _logger.LogDebug("Fetching active streaks for user {UserId}", userId);
        return await _dbSet
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.CurrentCount)
            .ToListAsync();
    }

    public async Task<PagedResult<Streak>> GetFilteredAsync(StreakFilterParams filters)
    {
        _logger.LogDebug("Fetching streaks with filters: UserId={UserId}, StreakType={StreakType}, IsActive={IsActive}",
            filters.UserId, filters.StreakType, filters.IsActive);

        var query = _dbSet.AsQueryable();

        if (filters.UserId.HasValue)
            query = query.Where(s => s.UserId == filters.UserId.Value);
        if (!string.IsNullOrEmpty(filters.StreakType))
            query = query.Where(s => s.StreakType == filters.StreakType);
        if (filters.IsActive.HasValue)
            query = query.Where(s => s.IsActive == filters.IsActive.Value);
        if (filters.MinCount.HasValue)
            query = query.Where(s => s.CurrentCount >= filters.MinCount.Value);

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderByDescending(s => s.CurrentCount)
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        _logger.LogDebug("Fetched {Count} streaks (total: {Total})", data.Count, totalCount);

        return new PagedResult<Streak>
        {
            Data = data,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> IncrementAsync(int id, DateTime? activityDate = null)
    {
        // Atomic SQL UPDATE. Previously this was load → modify in-memory →
        // SaveChanges, which raced badly when the user rapid-tapped a
        // check-in checkbox: two concurrent CheckInTask requests would each
        // load the streak into their own DbContext, both modify, and the
        // second SaveChanges threw DbUpdateConcurrencyException ("0 rows
        // affected") because EF's tracker had a stale view. ExecuteUpdateAsync
        // serializes at the DB row-lock level and bypasses the change
        // tracker entirely, so concurrent increments now reliably each
        // advance the count by 1.
        var date = activityDate ?? DateTime.UtcNow;
        _logger.LogInformation("Incrementing streak {StreakId}", id);
        var rowsAffected = await _dbSet
            .Where(s => s.StreakId == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.CurrentCount, s => s.CurrentCount + 1)
                .SetProperty(s => s.LongestCount, s =>
                    s.CurrentCount + 1 > s.LongestCount ? s.CurrentCount + 1 : s.LongestCount)
                .SetProperty(s => s.LastActivityDate, date)
                .SetProperty(s => s.IsActive, true)
                .SetProperty(s => s.BonusMultiplier, s =>
                    s.CurrentCount + 1 >= 30 ? 2.0m :
                    s.CurrentCount + 1 >= 14 ? 1.8m :
                    s.CurrentCount + 1 >= 7 ? 1.5m :
                    s.CurrentCount + 1 >= 3 ? 1.2m :
                    1.0m));
        if (rowsAffected == 0)
        {
            _logger.LogWarning("Streak {StreakId} not found for increment", id);
            return false;
        }
        // ExecuteUpdateAsync bypasses the change tracker, so any Streak
        // entity loaded earlier in the same request (e.g. via
        // GetByTaskIdAsync above CheckInTask's IncrementAsync call) still
        // shows the OLD column values. Detach so a follow-up GetByIdAsync
        // hits the DB and returns the freshly-incremented row.
        foreach (var entry in _context.ChangeTracker.Entries<Streak>().ToList())
        {
            if (entry.Entity.StreakId == id) entry.State = EntityState.Detached;
        }
        _logger.LogInformation("Streak {StreakId} incremented", id);
        return true;
    }

    public async Task<bool> RestoreAsync(int id, int currentCount, int longestCount, DateTime lastActivityDate, bool isActive, decimal bonusMultiplier)
    {
        // Atomic equivalent of UndoCheckIn's load-modify-UpdateAsync streak
        // restore. The load-modify pattern raced against a concurrent
        // check-in's IncrementAsync (both touched the same Streak row in
        // separate DbContexts) and threw DbUpdateConcurrencyException on
        // the slower one. ExecuteUpdateAsync serializes at the row level.
        var rowsAffected = await _dbSet
            .Where(s => s.StreakId == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.CurrentCount, currentCount)
                .SetProperty(s => s.LongestCount, longestCount)
                .SetProperty(s => s.LastActivityDate, lastActivityDate)
                .SetProperty(s => s.IsActive, isActive)
                .SetProperty(s => s.BonusMultiplier, bonusMultiplier));
        if (rowsAffected == 0) return false;
        // Detach stale tracked copies, mirroring IncrementAsync — keeps a
        // follow-up read in the same DbContext from returning pre-restore
        // column values.
        foreach (var entry in _context.ChangeTracker.Entries<Streak>().ToList())
        {
            if (entry.Entity.StreakId == id) entry.State = EntityState.Detached;
        }
        return true;
    }

    public async Task<Streak?> GetByTaskIdAsync(Guid taskId)
        => await _dbSet.FirstOrDefaultAsync(s => s.TaskId == taskId);

    public async Task<int> DeleteByTaskIdAsync(Guid taskId)
    {
        _logger.LogInformation("Deleting all streaks for task {TaskId}", taskId);
        var streaks = await _dbSet.Where(s => s.TaskId == taskId).ToListAsync();
        if (streaks.Count == 0) return 0;
        _dbSet.RemoveRange(streaks);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted {Count} streak(s) for task {TaskId}", streaks.Count, taskId);
        return streaks.Count;
    }

    public async Task<bool> ResetAsync(int id)
    {
        _logger.LogInformation("Resetting streak {StreakId}", id);
        var streak = await _dbSet.FindAsync(id);
        if (streak == null)
        {
            _logger.LogWarning("Streak {StreakId} not found for reset", id);
            return false;
        }

        streak.CurrentCount = 0;
        streak.BonusMultiplier = 1.0m;
        streak.IsActive = false;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Streak {StreakId} reset successfully", id);
        return true;
    }
}
