using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class MinigameRepository : Repository<Minigame, int>, IMinigameRepository
{
    public MinigameRepository(WahahaDbContext context, ILogger<MinigameRepository> logger)
        : base(context, logger) { }

    public async Task<IEnumerable<Minigame>> GetUnlockedAsync(int userLevel)
    {
        _logger.LogDebug("Fetching minigames unlocked for level {Level}", userLevel);
        return await _dbSet
            .Where(g => g.UnlockLevel <= userLevel)
            .OrderBy(g => g.UnlockLevel)
            .ToListAsync();
    }

    public async Task<IEnumerable<Minigame>> GetByDifficultyAsync(Difficulty difficulty)
    {
        _logger.LogDebug("Fetching minigames by difficulty {Difficulty}", difficulty);
        return await _dbSet
            .Where(g => g.Difficulty == difficulty)
            .ToListAsync();
    }

    public async Task<IEnumerable<Minigame>> GetFilteredAsync(MinigameFilterParams filters)
    {
        _logger.LogDebug("Fetching minigames with filters: Difficulty={Difficulty}, Type={Type}",
            filters.Difficulty, filters.Type);

        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(filters.Difficulty) && Enum.TryParse<Difficulty>(filters.Difficulty, true, out var difficulty))
            query = query.Where(g => g.Difficulty == difficulty);
        if (!string.IsNullOrEmpty(filters.Type) && Enum.TryParse<GameType>(filters.Type, true, out var type))
            query = query.Where(g => g.Type == type);
        if (filters.MinUnlockLevel.HasValue)
            query = query.Where(g => g.UnlockLevel >= filters.MinUnlockLevel.Value);
        if (filters.MaxUnlockLevel.HasValue)
            query = query.Where(g => g.UnlockLevel <= filters.MaxUnlockLevel.Value);

        return await query.OrderBy(g => g.UnlockLevel).ToListAsync();
    }
}
