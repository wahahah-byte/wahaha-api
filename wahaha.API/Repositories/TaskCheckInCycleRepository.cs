using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class TaskCheckInCycleRepository : Repository<TaskCheckInCycle, int>, ITaskCheckInCycleRepository
{
    public TaskCheckInCycleRepository(WahahaDbContext context, ILogger<TaskCheckInCycleRepository> logger)
        : base(context, logger) { }

    // Atomic delete via ExecuteDeleteAsync — replaces the base Find+Remove+
    // SaveChanges. Rapid check-in/undo cycles can produce two concurrent
    // delete attempts on the same row (the "Only most recent" tolerance in
    // UndoCheckIn re-targets stale undo requests to the actual latest cycle,
    // so two undo flows can converge on the same cycle); the tracker-based
    // delete throws DbUpdateConcurrencyException on the slower one because
    // it sees "0 rows affected". ExecuteDeleteAsync is idempotent — returns
    // 0 silently if the row is already gone, which is the desired outcome.
    public override async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting CheckIn cycle {CycleId}", id);
        var rows = await _dbSet.Where(c => c.CycleId == id).ExecuteDeleteAsync();
        foreach (var entry in _context.ChangeTracker.Entries<TaskCheckInCycle>().ToList())
        {
            if (entry.Entity.CycleId == id) entry.State = EntityState.Detached;
        }
        return rows > 0;
    }

    public async Task<(IEnumerable<TaskCheckInCycle> Items, int TotalCount)> GetByTaskIdAsync(Guid taskId, int pageNumber, int pageSize)
    {
        var query = _dbSet.Where(c => c.TaskId == taskId);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CheckInDate)
            .ThenByDescending(c => c.CycleId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<TaskCheckInCycle?> GetLatestByTaskIdAsync(Guid taskId)
    {
        return await _dbSet
            .Where(c => c.TaskId == taskId)
            .OrderByDescending(c => c.CheckInDate)
            .ThenByDescending(c => c.CycleId)
            .FirstOrDefaultAsync();
    }

    public async Task<TaskCheckInCycle?> GetLatestCheckinByTaskIdAsync(Guid taskId)
    {
        return await _dbSet
            .Where(c => c.TaskId == taskId && c.CycleType == "checkin")
            .OrderByDescending(c => c.CheckInDate)
            .ThenByDescending(c => c.CycleId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetDailyCounterSumAsync(Guid taskId, DateTime date)
    {
        return await _dbSet
            .Where(c => c.TaskId == taskId
                     && c.CheckInDate.Date == date.Date
                     && c.CounterValue.HasValue)
            .SumAsync(c => c.CounterValue!.Value);
    }
}
