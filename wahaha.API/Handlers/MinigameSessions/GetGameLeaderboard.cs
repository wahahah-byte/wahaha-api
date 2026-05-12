using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.MinigameSessions;

public sealed record GetGameLeaderboardRequest(int GameId);

public sealed class GetGameLeaderboardHandler : IRequestHandler<GetGameLeaderboardRequest, IEnumerable<MinigameSessionLeaderboardDto>>
{
    private readonly IMinigameSessionRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetGameLeaderboardHandler> _logger;

    public GetGameLeaderboardHandler(IMinigameSessionRepository repo, IMapper mapper, ILogger<GetGameLeaderboardHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<MinigameSessionLeaderboardDto>>> HandleAsync(GetGameLeaderboardRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching leaderboard for game {GameId}", request.GameId);
        var leaderboard = await _repo.GetLeaderboardAsync(request.GameId);
        return HandlerResult<IEnumerable<MinigameSessionLeaderboardDto>>.Ok(_mapper.Map<IEnumerable<MinigameSessionLeaderboardDto>>(leaderboard));
    }
}
