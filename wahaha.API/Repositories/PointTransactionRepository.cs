using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class PointTransactionRepository : Repository<PointTransaction, int>, IPointTransactionRepository
{
    public PointTransactionRepository(WahahaDbContext context) : base(context) { }

    public async Task<IEnumerable<PointTransaction>> GetByUserAsync(Guid userId)
        => await _dbSet
            .Where(pt => pt.UserId == userId)
            .OrderByDescending(pt => pt.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<PointTransaction>> GetByUserAndTypeAsync(Guid userId, TransactionType type)
        => await _dbSet
            .Where(pt => pt.UserId == userId && pt.Type == type)
            .OrderByDescending(pt => pt.CreatedAt)
            .ToListAsync();

    public async Task<PagedResult<PointTransaction>> GetFilteredAsync(PointTransactionFilterParams filters)
    {
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

        return new PagedResult<PointTransaction>
        {
            Data = data,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }
}
