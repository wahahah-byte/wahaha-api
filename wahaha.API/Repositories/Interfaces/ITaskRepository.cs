using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Repositories.Interfaces;

public interface ITaskRepository : IRepository<Models.Domain.Task, Guid>
{
    Task<IEnumerable<Models.Domain.Task>> GetByUserAsync(Guid userId);
    Task<IEnumerable<Models.Domain.Task>> GetPendingByUserAsync(Guid userId);
    Task<PagedResult<Models.Domain.Task>> GetFilteredAsync(TaskFilterParams filters);
    Task<IEnumerable<Models.Domain.Task>> GetPenaltyCandidatesAsync(DateTime cutoffDate);
    Task<bool> StartAsync(Guid id);
    Task<bool> CompleteAsync(Guid id);
    Task<bool> SetArchivedAsync(Guid id, bool isArchived);
    Task<int> AutoArchiveAsync(DateTime cutoffDate);
    // Atomic cycle-rollback setter for the undo flow.
    Task<bool> SetCycleStateAsync(Guid id, DateTime? dueDate, DateTime? lastCheckInDate);
}
