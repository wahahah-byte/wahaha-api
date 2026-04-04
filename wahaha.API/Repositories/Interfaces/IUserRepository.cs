using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Repositories.Interfaces;

public interface IUserRepository : IRepository<Users, Guid>
{
    Task<PagedResult<Users>> GetAllWithTransactionsAsync(UserFilterParams filters);
    Task<Users?> GetByIdWithTransactionsAsync(Guid id);
    Task<Users?> GetByUsernameAsync(string username);
    Task<bool> AddPointsAsync(Guid id, int points);
    Task<bool> SpendPointsAsync(Guid id, int points);
}
