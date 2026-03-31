using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class MinigameRepository : Repository<Minigame, int>, IMinigameRepository
{
    public MinigameRepository(WahahaDbContext context) : base(context) { }

    public async Task<IEnumerable<Minigame>> GetUnlockedAsync(int userLevel)
        => await _dbSet
            .Where(g => g.UnlockLevel <= userLevel)
            .OrderBy(g => g.UnlockLevel)
            .ToListAsync();

    public async Task<IEnumerable<Minigame>> GetByDifficultyAsync(Difficulty difficulty)
        => await _dbSet
            .Where(g => g.Difficulty == difficulty)
            .ToListAsync();
}
