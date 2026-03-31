using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class MinigameSessionRepository : Repository<MinigameSession, int>, IMinigameSessionRepository
{
    public MinigameSessionRepository(WahahaDbContext context) : base(context) { }

    public async Task<IEnumerable<MinigameSession>> GetByUserAsync(Guid userId)
        => await _dbSet
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.PlayedAt)
            .ToListAsync();

    public async Task<IEnumerable<MinigameSession>> GetByGameAsync(int gameId)
        => await _dbSet
            .Where(s => s.GameId == gameId)
            .OrderByDescending(s => s.Score)
            .ToListAsync();

    public async Task<IEnumerable<MinigameSession>> GetLeaderboardAsync(int gameId)
        => await _dbSet
            .Where(s => s.GameId == gameId && s.Outcome == SessionOutcome.WIN)
            .OrderByDescending(s => s.Score)
            .Take(10)
            .Include(s => s.User)
            .ToListAsync();
}
