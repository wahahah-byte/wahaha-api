using wahaha.API.Models.Domain;

namespace wahaha.API.Repositories.Interfaces;

public interface IMinigameRepository : IRepository<Minigame, int>
{
    Task<IEnumerable<Minigame>> GetUnlockedAsync(int userLevel);
    Task<IEnumerable<Minigame>> GetByDifficultyAsync(Difficulty difficulty);
}
