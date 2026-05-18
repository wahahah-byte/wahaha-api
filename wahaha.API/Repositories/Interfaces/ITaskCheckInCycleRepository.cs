using wahaha.API.Models.Domain;

namespace wahaha.API.Repositories.Interfaces;

public interface ITaskCheckInCycleRepository : IRepository<TaskCheckInCycle, int>
{
    Task<(IEnumerable<TaskCheckInCycle> Items, int TotalCount)> GetByTaskIdAsync(Guid taskId, int pageNumber, int pageSize);
    Task<TaskCheckInCycle?> GetLatestByTaskIdAsync(Guid taskId);
    // Latest cycle filtered to CycleType == "checkin". Used by the undo
    // endpoint so a later "log" cycle (counter quick-log) for the same day
    // doesn't disqualify the most recent check-in from being undone.
    Task<TaskCheckInCycle?> GetLatestCheckinByTaskIdAsync(Guid taskId);
    // Sums CounterValue across all cycles for the given task on the given date.
    // Used to enforce that a quick-log delta can't drive the running daily
    // total below zero.
    Task<int> GetDailyCounterSumAsync(Guid taskId, DateTime date);
}
