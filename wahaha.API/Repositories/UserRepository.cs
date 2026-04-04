using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class UserRepository : Repository<Users, Guid>, IUserRepository
{
    public UserRepository(WahahaDbContext context) : base(context) { }

    public async Task<PagedResult<Users>> GetAllWithTransactionsAsync(UserFilterParams filters)
    {
        var query = _dbSet.Include(u => u.PointTransactions).AsQueryable();

        if (filters.Level.HasValue)
            query = query.Where(u => u.Level == filters.Level.Value);

        if (filters.MinLevel.HasValue)
            query = query.Where(u => u.Level >= filters.MinLevel.Value);

        if (filters.MaxLevel.HasValue)
            query = query.Where(u => u.Level <= filters.MaxLevel.Value);

        var totalCount = await query.CountAsync();

        var data = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PagedResult<Users>
        {
            Data = data,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Users?> GetByIdWithTransactionsAsync(Guid id)
        => await _dbSet
            .Include(u => u.PointTransactions)
            .FirstOrDefaultAsync(u => u.UserId == id);

    public async Task<Users?> GetByUsernameAsync(string username)
        => await _dbSet
            .Include(u => u.PointTransactions)
            .FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> AddPointsAsync(Guid id, int points)
    {
        var user = await _dbSet.FindAsync(id);
        if (user == null) return false;

        user.CurrentBalance += points;
        user.TotalPointsEarned += points;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SpendPointsAsync(Guid id, int points)
    {
        var user = await _dbSet.FindAsync(id);
        if (user == null || user.CurrentBalance < points) return false;

        user.CurrentBalance -= points;

        await _context.SaveChangesAsync();
        return true;
    }
}
