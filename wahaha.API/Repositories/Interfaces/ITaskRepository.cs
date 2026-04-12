using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Repositories.Interfaces;

public interface ITaskRepository : IRepository<Models.Domain.Task, Guid>
{
    Task<IEnumerable<Models.Domain.Task>> GetByUserAsync(Guid userId);
    Task<IEnumerable<Models.Domain.Task>> GetPendingByUserAsync(Guid userId);
    Task<PagedResult<Models.Domain.Task>> GetFilteredAsync(TaskFilterParams filters);
    Task<bool> StartAsync(Guid id);
    Task<bool> CompleteAsync(Guid id);
}
