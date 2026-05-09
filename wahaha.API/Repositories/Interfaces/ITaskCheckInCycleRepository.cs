using wahaha.API.Models.Domain;

namespace wahaha.API.Repositories.Interfaces;

public interface ITaskCheckInCycleRepository : IRepository<TaskCheckInCycle, int>
{
    Task<(IEnumerable<TaskCheckInCycle> Items, int TotalCount)> GetByTaskIdAsync(Guid taskId, int pageNumber, int pageSize);
    Task<TaskCheckInCycle?> GetLatestByTaskIdAsync(Guid taskId);
}
