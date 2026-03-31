using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class PointTransactionRepository : Repository<PointTransaction, int>, IPointTransactionRepository
{
    public PointTransactionRepository(WahahaDbContext context) : base(context) { }

    public async Task<IEnumerable<PointTransaction>> GetByUserAsync(Guid userId)
        => await _dbSet
            .Where(pt => pt.UserId == userId)
            .OrderByDescending(pt => pt.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<PointTransaction>> GetByUserAndTypeAsync(Guid userId, TransactionType type)
        => await _dbSet
            .Where(pt => pt.UserId == userId && pt.Type == type)
            .OrderByDescending(pt => pt.CreatedAt)
            .ToListAsync();
}
