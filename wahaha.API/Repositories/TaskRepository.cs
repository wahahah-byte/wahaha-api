using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
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

    public async Task<bool> CompleteAsync(Guid id)
    {
        var task = await _dbSet.FindAsync(id);
        if (task == null) return false;

        if (task.Status == ByteTaskStatus.completed)
            return false;

        task.Status = ByteTaskStatus.completed;
        task.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
