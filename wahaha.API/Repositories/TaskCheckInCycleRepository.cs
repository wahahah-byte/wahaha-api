using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class TaskCheckInCycleRepository : Repository<TaskCheckInCycle, int>, ITaskCheckInCycleRepository
{
    public TaskCheckInCycleRepository(WahahaDbContext context, ILogger<TaskCheckInCycleRepository> logger)
        : base(context, logger) { }

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
}
