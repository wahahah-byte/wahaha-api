using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class Repository<T, TKey> : IRepository<T, TKey> where T : class
{
    protected readonly WahahaDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger _logger;

    public Repository(WahahaDbContext context, ILogger logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all {Entity}", typeof(T).Name);
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(TKey id)
    {
        _logger.LogDebug("Fetching {Entity} with ID {Id}", typeof(T).Name, id);
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        _logger.LogInformation("Creating new {Entity}", typeof(T).Name);
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T?> UpdateAsync(T entity)
    {
        _logger.LogInformation("Updating {Entity}", typeof(T).Name);
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(TKey id)
    {
        _logger.LogInformation("Deleting {Entity} with ID {Id}", typeof(T).Name, id);
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
        {
            _logger.LogWarning("{Entity} with ID {Id} not found for deletion", typeof(T).Name, id);
            return false;
        }

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
