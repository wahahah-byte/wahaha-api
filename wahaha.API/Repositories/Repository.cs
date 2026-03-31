using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class Repository<T, TKey> : IRepository<T, TKey> where T : class
{
    protected readonly WahahaDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(WahahaDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public virtual async Task<T?> GetByIdAsync(TKey id)
        => await _dbSet.FindAsync(id);

    public virtual async Task<T> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T?> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(TKey id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
