using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class PointTransactionRepository : Repository<PointTransaction, int>, IPointTransactionRepository
{
    public PointTransactionRepository(WahahaDbContext context, ILogger<PointTransactionRepository> logger)
        : base(context, logger) { }

    // Atomic delete via ExecuteDeleteAsync — same rationale as
    // TaskCheckInCycleRepository.DeleteAsync. Concurrent undo flows can both
    // attempt to delete the same PointTransaction (the cycle they're each
    // undoing has the same PointTransactionId after the latest-cycle swap);
    // the tracker-based delete throws DbUpdateConcurrencyException on the
    // slower one, while ExecuteDeleteAsync is idempotent.
    public override async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting PointTransaction {TransactionId}", id);
        var rows = await _dbSet.Where(pt => pt.TransactionId == id).ExecuteDeleteAsync();
        foreach (var entry in _context.ChangeTracker.Entries<PointTransaction>().ToList())
        {
            if (entry.Entity.TransactionId == id) entry.State = EntityState.Detached;
        }
        return rows > 0;
    }

    public async Task<IEnumerable<PointTransaction>> GetByUserAsync(Guid userId)
    {
        _logger.LogDebug("Fetching point transactions for user {UserId}", userId);
        return await _dbSet
            .Where(pt => pt.UserId == userId)
            .OrderByDescending(pt => pt.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PointTransaction>> GetByUserAndTypeAsync(Guid userId, TransactionType type)
    {
        _logger.LogDebug("Fetching {Type} transactions for user {UserId}", type, userId);
        return await _dbSet
            .Where(pt => pt.UserId == userId && pt.Type == type)
            .OrderByDescending(pt => pt.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetDailyEarnedAsync(Guid userId, DateTime utcDate)
    {
        var dayStart = utcDate.Date;
        var dayEnd = dayStart.AddDays(1);
        var set = _dbSet.Where(pt => pt.UserId == userId
                      && pt.Type == TransactionType.EARN
                      && pt.CreatedAt >= dayStart
                      && pt.CreatedAt < dayEnd);
        return set.Sum(pt => pt.Amount);
    }

    public async Task<int> GetDailyEarnedBySourceTypeAsync(Guid userId, DateTime utcDate, SourceType sourceType)
    {
        var dayStart = utcDate.Date;
        var dayEnd = dayStart.AddDays(1);
        return await _dbSet
            .Where(pt => pt.UserId == userId
                      && pt.Type == TransactionType.EARN
                      && pt.SourceType == sourceType
                      && pt.CreatedAt >= dayStart
                      && pt.CreatedAt < dayEnd)
            .SumAsync(pt => pt.Amount);
    }

    public async Task<int> GetDailyEarnedByCategoryAsync(Guid userId, DateTime utcDate, string category, SourceType sourceType)
    {
        var dayStart = utcDate.Date;
        var dayEnd = dayStart.AddDays(1);
        return await _dbSet
            .Where(pt => pt.UserId == userId
                      && pt.Type == TransactionType.EARN
                      && pt.SourceType == sourceType
                      && pt.Category == category
                      && pt.CreatedAt >= dayStart
                      && pt.CreatedAt < dayEnd)
            .SumAsync(pt => pt.Amount);
    }

    // CheckInTask needs both the source-type total AND the per-category total
    // to enforce the daily caps. Compute both in one query so the handler
    // doesn't pay two round trips for what is essentially the same scan.
    public async Task<(int BySourceType, int InCategory)> GetDailyEarnedTotalsAsync(
        Guid userId, DateTime utcDate, SourceType sourceType, string category)
    {
        var dayStart = utcDate.Date;
        var dayEnd = dayStart.AddDays(1);
        var totals = await _dbSet
            .Where(pt => pt.UserId == userId
                      && pt.Type == TransactionType.EARN
                      && pt.SourceType == sourceType
                      && pt.CreatedAt >= dayStart
                      && pt.CreatedAt < dayEnd)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                BySourceType = g.Sum(pt => pt.Amount),
                InCategory = g.Sum(pt => pt.Category == category ? pt.Amount : 0),
            })
            .FirstOrDefaultAsync();
        return (totals?.BySourceType ?? 0, totals?.InCategory ?? 0);
    }

    public async Task<PagedResult<PointTransaction>> GetFilteredAsync(PointTransactionFilterParams filters)
    {
        _logger.LogDebug("Fetching point transactions with filters: UserId={UserId}, Type={Type}, SourceType={SourceType}",
            filters.UserId, filters.Type, filters.SourceType);

        var query = _dbSet.AsQueryable();

        if (filters.UserId.HasValue)
            query = query.Where(pt => pt.UserId == filters.UserId.Value);
        if (!string.IsNullOrEmpty(filters.Type) && Enum.TryParse<TransactionType>(filters.Type, true, out var type))
            query = query.Where(pt => pt.Type == type);
        if (!string.IsNullOrEmpty(filters.SourceType) && Enum.TryParse<SourceType>(filters.SourceType, true, out var sourceType))
            query = query.Where(pt => pt.SourceType == sourceType);
        if (filters.FromDate.HasValue)
            query = query.Where(pt => pt.CreatedAt >= filters.FromDate.Value);
        if (filters.ToDate.HasValue)
            query = query.Where(pt => pt.CreatedAt <= filters.ToDate.Value);

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderByDescending(pt => pt.CreatedAt)
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        _logger.LogDebug("Fetched {Count} transactions (total: {Total})", data.Count, totalCount);

        return new PagedResult<PointTransaction>
        {
            Data = data,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }
}
