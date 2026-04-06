using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class UserRepository : Repository<Users, Guid>, IUserRepository
{
    public UserRepository(WahahaDbContext context, ILogger<UserRepository> logger)
        : base(context, logger) { }

    public async Task<PagedResult<Users>> GetAllWithTransactionsAsync(UserFilterParams filters)
    {
        _logger.LogDebug("Fetching users with filters: Level={Level}, MinLevel={MinLevel}, MaxLevel={MaxLevel}",
            filters.Level, filters.MinLevel, filters.MaxLevel);

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

        _logger.LogDebug("Fetched {Count} users (total: {Total})", data.Count, totalCount);

        return new PagedResult<Users>
        {
            Data = data,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Users?> GetByIdWithTransactionsAsync(Guid id)
    {
        _logger.LogDebug("Fetching user with transactions for ID {UserId}", id);
        return await _dbSet
            .Include(u => u.PointTransactions)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<Users?> GetByUsernameAsync(string username)
    {
        _logger.LogDebug("Fetching user by username {Username}", username);
        return await _dbSet
            .Include(u => u.PointTransactions)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> AddPointsAsync(Guid id, int points)
    {
        _logger.LogInformation("Adding {Points} points to user {UserId}", points, id);
        var user = await _dbSet.FindAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found when adding points", id);
            return false;
        }

        user.CurrentBalance += points;
        user.TotalPointsEarned += points;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SpendPointsAsync(Guid id, int points)
    {
        _logger.LogInformation("Spending {Points} points for user {UserId}", points, id);
        var user = await _dbSet.FindAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found when spending points", id);
            return false;
        }

        if (user.CurrentBalance < points)
        {
            _logger.LogWarning("User {UserId} has insufficient balance ({Balance}) to spend {Points} points",
                id, user.CurrentBalance, points);
            return false;
        }

        user.CurrentBalance -= points;
        await _context.SaveChangesAsync();
        return true;
    }
}
