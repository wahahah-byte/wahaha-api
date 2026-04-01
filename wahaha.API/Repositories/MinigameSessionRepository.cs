using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class MinigameSessionRepository : Repository<MinigameSession, int>, IMinigameSessionRepository
{
    public MinigameSessionRepository(WahahaDbContext context) : base(context) { }

    public override async Task<IEnumerable<MinigameSession>> GetAllAsync()
        => await _dbSet
            .Include(s => s.Minigame)
            .Include(s => s.User)
            .ToListAsync();

    public async Task<IEnumerable<MinigameSession>> GetByUserAsync(Guid userId)
        => await _dbSet
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.PlayedAt)
            .Include(s => s.Minigame)
            .ToListAsync();

    public async Task<IEnumerable<MinigameSession>> GetByGameAsync(int gameId)
        => await _dbSet
            .Where(s => s.GameId == gameId)
            .OrderByDescending(s => s.Score)
            .Include(s => s.Minigame)
            .Include(s => s.User)
            .ToListAsync();

    public async Task<IEnumerable<MinigameSession>> GetLeaderboardAsync(int gameId)
        => await _dbSet
            .Where(s => s.GameId == gameId && s.Outcome == SessionOutcome.WIN)
            .OrderByDescending(s => s.Score)
            .Take(10)
            .Include(s => s.Minigame)
            .Include(s => s.User)
            .ToListAsync();
}