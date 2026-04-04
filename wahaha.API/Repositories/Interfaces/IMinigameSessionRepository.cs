using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;

namespace wahaha.API.Repositories.Interfaces;

public interface IMinigameSessionRepository : IRepository<MinigameSession, int>
{
    Task<IEnumerable<MinigameSession>> GetByUserAsync(Guid userId);
    Task<IEnumerable<MinigameSession>> GetByGameAsync(int gameId);
    Task<IEnumerable<MinigameSession>> GetLeaderboardAsync(int gameId);
    Task<IEnumerable<MinigameSession>> GetFilteredAsync(MinigameSessionFilterParams filters);
}
