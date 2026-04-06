using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class TaskRepository : Repository<Models.Domain.Task, Guid>, ITaskRepository
{
    public TaskRepository(WahahaDbContext context, ILogger<TaskRepository> logger)
        : base(context, logger) { }

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
        _logger.LogDebug("Fetching tasks with filters: UserId={UserId}, Status={Status}, Priority={Priority}, Category={Category}",
            filters.UserId, filters.Status, filters.Priority, filters.Category);

        var query = _dbSet.AsQueryable();

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
}
