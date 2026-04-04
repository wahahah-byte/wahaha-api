using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;

namespace wahaha.API.Repositories.Interfaces;

public interface IMinigameRepository : IRepository<Minigame, int>
{
    Task<IEnumerable<Minigame>> GetUnlockedAsync(int userLevel);
    Task<IEnumerable<Minigame>> GetByDifficultyAsync(Difficulty difficulty);
    Task<IEnumerable<Minigame>> GetFilteredAsync(MinigameFilterParams filters);
}
