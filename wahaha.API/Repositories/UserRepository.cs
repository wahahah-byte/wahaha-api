using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class UserRepository : Repository<Users, Guid>, IUserRepository
{
    public UserRepository(WahahaDbContext context) : base(context) { }

    public async Task<IEnumerable<Users>> GetAllWithTransactionsAsync()
        => await _dbSet
            .Include(u => u.PointTransactions)
            .ToListAsync();

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
