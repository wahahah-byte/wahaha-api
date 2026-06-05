using wahaha.API.Models.Domain;

namespace wahaha.API.Repositories.Interfaces;

public interface ITaskCheckInCycleRepository : IRepository<TaskCheckInCycle, int>
{
    Task<(IEnumerable<TaskCheckInCycle> Items, int TotalCount)> GetByTaskIdAsync(Guid taskId, int pageNumber, int pageSize);
    Task<TaskCheckInCycle?> GetLatestByTaskIdAsync(Guid taskId);
    // Latest cycle filtered to CycleType == "checkin" for the undo endpoint.
    Task<TaskCheckInCycle?> GetLatestCheckinByTaskIdAsync(Guid taskId);
    // Daily CounterValue sum used to guard quick-log deltas from going negative.
    Task<int> GetDailyCounterSumAsync(Guid taskId, DateTime date);
    // Removes today's "log" cycles so the handler can consolidate them into one checkin cycle; returns rows deleted.
    Task<int> DeleteDailyLogsAsync(Guid taskId, DateTime date);
}
