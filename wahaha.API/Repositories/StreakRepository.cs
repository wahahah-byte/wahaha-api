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

    public async Task<bool> IncrementAsync(int id)
    {
        _logger.LogInformation("Incrementing streak {StreakId}", id);
        var streak = await _dbSet.FindAsync(id);
        if (streak == null)
        {
            _logger.LogWarning("Streak {StreakId} not found for increment", id);
            return false;
        }

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
        _logger.LogInformation("Streak {StreakId} incremented to {Count}", id, streak.CurrentCount);
        return true;
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
