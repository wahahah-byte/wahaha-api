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
}
