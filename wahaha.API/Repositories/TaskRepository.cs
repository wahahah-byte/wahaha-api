using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class TaskRepository : Repository<Models.Domain.Task, Guid>, ITaskRepository
{
    public TaskRepository(WahahaDbContext context) : base(context) { }

    public async Task<IEnumerable<Models.Domain.Task>> GetByUserAsync(Guid userId)
        => await _dbSet
            .Where(t => t.UserId == userId)
            .ToListAsync();

    public async Task<IEnumerable<Models.Domain.Task>> GetPendingByUserAsync(Guid userId)
        => await _dbSet
            .Where(t => t.UserId == userId && t.Status == ByteTaskStatus.pending)
            .OrderByDescending(t => t.Priority)
            .ToListAsync();

    public async Task<PagedResult<Models.Domain.Task>> GetFilteredAsync(TaskFilterParams filters)
    {
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
        var task = await _dbSet.FindAsync(id);
        if (task == null || task.Status == ByteTaskStatus.completed) return false;

        task.Status = ByteTaskStatus.completed;
        task.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
