using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.MinigameSessions;

public sealed record GetSessionsByGameRequest(int GameId, Guid UserId);

public sealed class GetSessionsByGameHandler : IRequestHandler<GetSessionsByGameRequest, IEnumerable<MinigameSessionDto>>
{
    private readonly IMinigameSessionRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSessionsByGameHandler> _logger;

    public GetSessionsByGameHandler(IMinigameSessionRepository repo, IMapper mapper, ILogger<GetSessionsByGameHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<MinigameSessionDto>>> HandleAsync(GetSessionsByGameRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching sessions for game {GameId} by user {UserId}", request.GameId, request.UserId);
        var filters = new MinigameSessionFilterParams { UserId = request.UserId, GameId = request.GameId };
        var sessions = await _repo.GetFilteredAsync(filters);
        return HandlerResult<IEnumerable<MinigameSessionDto>>.Ok(_mapper.Map<IEnumerable<MinigameSessionDto>>(sessions));
    }
}
