using wahaha.API.Models.Domain;

namespace wahaha.API.Repositories.Interfaces;

public interface IUserRepository : IRepository<Users, Guid>
{
    Task<Users?> GetByUsernameAsync(string username);
    Task<Users?> GetByIdWithTransactionsAsync(Guid id);
    Task<IEnumerable<Users>> GetAllWithTransactionsAsync();
    Task<bool> AddPointsAsync(Guid id, int points);
    Task<bool> SpendPointsAsync(Guid id, int points);
}
