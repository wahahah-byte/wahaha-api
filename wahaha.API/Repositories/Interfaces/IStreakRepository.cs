using wahaha.API.Models.Domain;

namespace wahaha.API.Repositories.Interfaces;

public interface IStreakRepository : IRepository<Streak, int>
{
    Task<IEnumerable<Streak>> GetByUserAsync(Guid userId);
    Task<IEnumerable<Streak>> GetActiveByUserAsync(Guid userId);
    Task<bool> IncrementAsync(int id);
    Task<bool> ResetAsync(int id);
}
