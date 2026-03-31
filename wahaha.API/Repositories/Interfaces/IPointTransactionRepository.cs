using wahaha.API.Models.Domain;

namespace wahaha.API.Repositories.Interfaces;

public interface IPointTransactionRepository : IRepository<PointTransaction, int>
{
    Task<IEnumerable<PointTransaction>> GetByUserAsync(Guid userId);
    Task<IEnumerable<PointTransaction>> GetByUserAndTypeAsync(Guid userId, TransactionType type);
}
