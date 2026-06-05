using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class TaskCheckInCycleRepository : Repository<TaskCheckInCycle, int>, ITaskCheckInCycleRepository
{
    public TaskCheckInCycleRepository(WahahaDbContext context, ILogger<TaskCheckInCycleRepository> logger)
        : base(context, logger) { }

    // Idempotent atomic delete via ExecuteDeleteAsync to survive concurrent undo races.
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

    public async Task<int> DeleteDailyLogsAsync(Guid taskId, DateTime date)
    {
        var rows = await _dbSet
            .Where(c => c.TaskId == taskId
                     && c.CheckInDate.Date == date.Date
                     && c.CycleType == "log")
            .ExecuteDeleteAsync();
        // Detach tracked entities we just wiped so a follow-up SaveChanges in the same tx doesn't re-mutate them.
        foreach (var entry in _context.ChangeTracker.Entries<TaskCheckInCycle>().ToList())
        {
            if (entry.Entity.TaskId == taskId
                && entry.Entity.CheckInDate.Date == date.Date
                && entry.Entity.CycleType == "log")
            {
                entry.State = EntityState.Detached;
            }
        }
        return rows;
    }
}
