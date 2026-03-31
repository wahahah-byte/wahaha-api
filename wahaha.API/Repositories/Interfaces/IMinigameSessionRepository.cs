using wahaha.API.Models.Domain;

namespace wahaha.API.Repositories.Interfaces;

public interface IMinigameSessionRepository : IRepository<MinigameSession, int>
{
    Task<IEnumerable<MinigameSession>> GetByUserAsync(Guid userId);
    Task<IEnumerable<MinigameSession>> GetByGameAsync(int gameId);
    Task<IEnumerable<MinigameSession>> GetLeaderboardAsync(int gameId);
}
