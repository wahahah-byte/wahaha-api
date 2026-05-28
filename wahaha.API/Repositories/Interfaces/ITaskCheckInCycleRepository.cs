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
    // Removes today's "log" cycles for a task so the check-in handler can
    // consolidate them into a single checkin cycle when the client supplies
    // an absolute target. Returns the number of rows deleted.
    Task<int> DeleteDailyLogsAsync(Guid taskId, DateTime date);
}
