using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class SubtaskRepository : Repository<Subtask, int>, ISubtaskRepository
{
    public SubtaskRepository(WahahaDbContext context, ILogger<SubtaskRepository> logger)
        : base(context, logger) { }

    public async Task<IEnumerable<Subtask>> GetByTaskIdAsync(Guid taskId)
    {
        return await _dbSet
            .Where(s => s.TaskId == taskId)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> DeleteByTaskIdAsync(Guid taskId)
    {
        var subtasks = await _dbSet.Where(s => s.TaskId == taskId).ToListAsync();
        if (subtasks.Count == 0) return 0;
        _dbSet.RemoveRange(subtasks);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted {Count} subtask(s) for task {TaskId}", subtasks.Count, taskId);
        return subtasks.Count;
    }

    public async Task<int> GetNextSortOrderAsync(Guid taskId)
    {
        var max = await _dbSet
            .Where(s => s.TaskId == taskId)
            .Select(s => (int?)s.SortOrder)
            .MaxAsync();
        return (max ?? -1) + 1;
    }

    public async Task<int> ResetCompletionByTaskIdAsync(Guid taskId)
    {
        // Atomic SQL UPDATE — was previously SELECT + per-row modify +
        // SaveChanges, which cost two round trips per check-in. Filtering on
        // (Completed || SetsCompleted != null) in the WHERE clause means we
        // only touch rows that actually need resetting.
        var changed = await _dbSet
            .Where(s => s.TaskId == taskId && (s.Completed || s.SetsCompleted != null))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Completed, false)
                .SetProperty(s => s.SetsCompleted, (int?)null));
        if (changed > 0)
        {
            _logger.LogInformation("Reset completion on {Count} subtask(s) for task {TaskId}", changed, taskId);
        }
        // Detach any tracked Subtask rows belonging to this task so any
        // follow-up read in the same DbContext sees the post-reset DB state
        // rather than the cached pre-reset entities (mirrors what
        // StreakRepository.IncrementAsync does for the same reason).
        foreach (var entry in _context.ChangeTracker.Entries<Subtask>().ToList())
        {
            if (entry.Entity.TaskId == taskId) entry.State = EntityState.Detached;
        }
        return changed;
    }
}
