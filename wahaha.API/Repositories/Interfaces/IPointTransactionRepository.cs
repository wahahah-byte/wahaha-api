using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Repositories.Interfaces;

public interface IPointTransactionRepository : IRepository<PointTransaction, int>
{
    Task<IEnumerable<PointTransaction>> GetByUserAsync(Guid userId);
    Task<IEnumerable<PointTransaction>> GetByUserAndTypeAsync(Guid userId, TransactionType type);
    Task<PagedResult<PointTransaction>> GetFilteredAsync(PointTransactionFilterParams filters);
    Task<int> GetDailyEarnedAsync(Guid userId, DateTime utcDate);
    Task<int> GetDailyEarnedBySourceTypeAsync(Guid userId, DateTime utcDate, SourceType sourceType);
    Task<int> GetDailyEarnedByCategoryAsync(Guid userId, DateTime utcDate, string category, SourceType sourceType);
    // Combined version of the two above — returns both totals from a single
    // query, used by CheckInTask which needs both for daily-cap validation.
    Task<(int BySourceType, int InCategory)> GetDailyEarnedTotalsAsync(
        Guid userId, DateTime utcDate, SourceType sourceType, string category);
}
