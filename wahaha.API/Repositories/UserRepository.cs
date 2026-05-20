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

    // Email match is case-insensitive at the DB layer (Latin1_General_CI_AS).
    public async Task<Users?> GetByEmailAsync(string email)
    {
        _logger.LogDebug("Fetching user by email {Email}", email);
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> AddPointsAsync(Guid id, int points)
    {
        // Atomic increment via ExecuteUpdateAsync to serialize concurrent point mutations.
        _logger.LogInformation("Adding {Points} points to user {UserId}", points, id);
        var rowsAffected = await _dbSet
            .Where(u => u.UserId == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.CurrentBalance, u => u.CurrentBalance + points)
                .SetProperty(u => u.TotalPointsEarned, u => u.TotalPointsEarned + points));
        if (rowsAffected == 0) { _logger.LogWarning("User {UserId} not found when adding points", id); return false; }
        DetachUser(id);
        return true;
    }

    public async Task<bool> SpendPointsAsync(Guid id, int points)
    {
        // Atomic conditional decrement; WHERE doubles as the balance guard.
        _logger.LogInformation("Spending {Points} points for user {UserId}", points, id);
        var rowsAffected = await _dbSet
            .Where(u => u.UserId == id && u.CurrentBalance >= points)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.CurrentBalance, u => u.CurrentBalance - points));
        if (rowsAffected == 0)
        {
            var existing = await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
            if (existing == null) { _logger.LogWarning("User {UserId} not found when spending points", id); return false; }
            _logger.LogWarning("User {UserId} has insufficient balance ({Balance}) to spend {Points} points",
                id, existing.CurrentBalance, points);
            return false;
        }
        DetachUser(id);
        return true;
    }

    // Reverse of AddPointsAsync; balance may go negative if user already spent the award.
    public async Task<bool> RefundPointsAsync(Guid id, int points)
    {
        _logger.LogInformation("Refunding {Points} points from user {UserId}", points, id);
        var rowsAffected = await _dbSet
            .Where(u => u.UserId == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.CurrentBalance, u => u.CurrentBalance - points)
                .SetProperty(u => u.TotalPointsEarned, u => u.TotalPointsEarned - points));
        if (rowsAffected == 0) { _logger.LogWarning("User {UserId} not found when refunding points", id); return false; }
        DetachUser(id);
        return true;
    }

    // Detach tracked User so follow-up reads see post-ExecuteUpdate balance.
    private void DetachUser(Guid id)
    {
        foreach (var entry in _context.ChangeTracker.Entries<Users>().ToList())
        {
            if (entry.Entity.UserId == id) entry.State = EntityState.Detached;
        }
    }
}
