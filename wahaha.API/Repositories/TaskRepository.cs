using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class TaskRepository : Repository<Models.Domain.Task, Guid>, ITaskRepository
{
    // Top N most-recent check-in cycles included with each Task so the detail modal
    // has counter history available on first paint without a follow-up fetch, and
    // the heatmap strip can render a full 14-day window for daily/weekdays tasks.
    private const int RecentCyclesPreviewCount = 14;

    public TaskRepository(WahahaDbContext context, ILogger<TaskRepository> logger)
        : base(context, logger) { }

    public override async Task<Models.Domain.Task?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.Subtasks)
            .Include(t => t.CheckInCycles
                .OrderByDescending(c => c.CheckInDate)
                .ThenByDescending(c => c.CycleId)
                .Take(RecentCyclesPreviewCount))
            .FirstOrDefaultAsync(t => t.TaskId == id);
    }

    public async Task<IEnumerable<Models.Domain.Task>> GetByUserAsync(Guid userId)
    {
        _logger.LogDebug("Fetching tasks for user {UserId}", userId);
        return await _dbSet
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Domain.Task>> GetPendingByUserAsync(Guid userId)
    {
        _logger.LogDebug("Fetching pending tasks for user {UserId}", userId);
        return await _dbSet
            .Where(t => t.UserId == userId && t.Status == ByteTaskStatus.pending)
            .OrderByDescending(t => t.Priority)
            .ToListAsync();
    }

    public async Task<PagedResult<Models.Domain.Task>> GetFilteredAsync(TaskFilterParams filters)
    {
        _logger.LogDebug("Fetching tasks with filters: UserId={UserId}, Status={Status}, Priority={Priority}, Category={Category}, IsArchived={IsArchived}",
            filters.UserId, filters.Status, filters.Priority, filters.Category, filters.IsArchived);

        IQueryable<Models.Domain.Task> query;
        if (filters.UserId.HasValue)
        {
            var uid = filters.UserId.Value;
            query = _dbSet
                .Include(t => t.Streaks.Where(s => s.UserId == uid && s.IsActive))
                .Include(t => t.Subtasks)
                .Include(t => t.CheckInCycles
                    .OrderByDescending(c => c.CheckInDate)
                    .ThenByDescending(c => c.CycleId)
                    .Take(RecentCyclesPreviewCount));
        }
        else
        {
            query = _dbSet
                .Include(t => t.Subtasks)
                .Include(t => t.CheckInCycles
                    .OrderByDescending(c => c.CheckInDate)
                    .ThenByDescending(c => c.CycleId)
                    .Take(RecentCyclesPreviewCount));
        }

        if (filters.UserId.HasValue)
            query = query.Where(t => t.UserId == filters.UserId.Value);
        if (!string.IsNullOrEmpty(filters.Status) && Enum.TryParse<ByteTaskStatus>(filters.Status, true, out var status))
            query = query.Where(t => t.Status == status);
        if (!string.IsNullOrEmpty(filters.Priority) && Enum.TryParse<Priority>(filters.Priority, true, out var priority))
            query = query.Where(t => t.Priority == priority);
        if (!string.IsNullOrEmpty(filters.Category))
            query = query.Where(t => t.Category.ToLower() == filters.Category.ToLower());
        if (filters.IsRecurring.HasValue)
            query = query.Where(t => t.IsRecurring == filters.IsRecurring.Value);

        // Default: exclude archived unless caller explicitly opts in (or asks for archived only).
        var archivedFilter = filters.IsArchived ?? false;
        query = query.Where(t => t.IsArchived == archivedFilter);

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderByDescending(t => t.Priority)
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        _logger.LogDebug("Fetched {Count} tasks (total: {Total})", data.Count, totalCount);

        return new PagedResult<Models.Domain.Task>
        {
            Data = data,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<Models.Domain.Task>> GetPenaltyCandidatesAsync(DateTime cutoffDate)
    {
        _logger.LogDebug("Fetching penalty candidates with due date before {Cutoff:yyyy-MM-dd}", cutoffDate);
        return await _dbSet
            .Where(t => t.Status == ByteTaskStatus.in_progress
                     && !t.IsRecurring
                     && t.DueDate.HasValue
                     && t.DueDate.Value < cutoffDate)
            .ToListAsync();
    }

    public async Task<bool> StartAsync(Guid id)
    {
        _logger.LogInformation("Starting task {TaskId}", id);
        var task = await _dbSet.FindAsync(id);

        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for start", id);
            return false;
        }

        if (task.Status != ByteTaskStatus.pending)
        {
            _logger.LogWarning("Task {TaskId} cannot be started — current status is {Status}", id, task.Status);
            return false;
        }

        task.Status = ByteTaskStatus.in_progress;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} started successfully", id);
        return true;
    }

    public async Task<bool> CompleteAsync(Guid id)
    {
        _logger.LogInformation("Completing task {TaskId}", id);
        var task = await _dbSet.FindAsync(id);

        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for completion", id);
            return false;
        }

        if (task.Status == ByteTaskStatus.completed)
        {
            _logger.LogWarning("Task {TaskId} is already completed", id);
            return false;
        }

        task.Status = ByteTaskStatus.completed;
        task.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Task {TaskId} completed successfully", id);
        return true;
    }

    public async Task<bool> SetArchivedAsync(Guid id, bool isArchived)
    {
        var task = await _dbSet.FindAsync(id);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for archive flip to {IsArchived}", id, isArchived);
            return false;
        }
        if (task.IsArchived == isArchived) return true;

        task.IsArchived = isArchived;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Task {TaskId} archive flag set to {IsArchived}", id, isArchived);
        return true;
    }

    public async Task<int> AutoArchiveAsync(DateTime cutoffDate)
    {
        _logger.LogDebug("Auto-archiving completed tasks with completed_at < {Cutoff:yyyy-MM-dd}", cutoffDate);
        var candidates = await _dbSet
            .Where(t => !t.IsArchived
                     && t.Status == ByteTaskStatus.completed
                     && t.CompletedAt.HasValue
                     && t.CompletedAt.Value < cutoffDate)
            .ToListAsync();

        if (candidates.Count == 0) return 0;

        foreach (var t in candidates) t.IsArchived = true;
        await _context.SaveChangesAsync();
        return candidates.Count;
    }
}
