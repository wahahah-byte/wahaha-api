using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Repositories.Interfaces;

public interface IStreakRepository : IRepository<Streak, int>
{
    Task<IEnumerable<Streak>> GetByUserAsync(Guid userId);
    Task<IEnumerable<Streak>> GetActiveByUserAsync(Guid userId);
    Task<PagedResult<Streak>> GetFilteredAsync(StreakFilterParams filters);
    Task<bool> IncrementAsync(int id, DateTime? activityDate = null);
    Task<bool> ResetAsync(int id);
    Task<Streak?> GetByTaskIdAsync(Guid taskId);
    Task<int> DeleteByTaskIdAsync(Guid taskId);
}
