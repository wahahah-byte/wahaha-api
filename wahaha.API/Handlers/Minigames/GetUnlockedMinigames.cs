using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Minigames;

public sealed record GetUnlockedMinigamesRequest(int UserLevel);

public sealed class GetUnlockedMinigamesHandler : IRequestHandler<GetUnlockedMinigamesRequest, IEnumerable<MinigameDto>>
{
    private readonly IMinigameRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUnlockedMinigamesHandler> _logger;

    public GetUnlockedMinigamesHandler(IMinigameRepository repo, IMapper mapper, ILogger<GetUnlockedMinigamesHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<MinigameDto>>> HandleAsync(GetUnlockedMinigamesRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching minigames unlocked for level {Level}", request.UserLevel);
        var games = await _repo.GetUnlockedAsync(request.UserLevel);
        return HandlerResult<IEnumerable<MinigameDto>>.Ok(_mapper.Map<IEnumerable<MinigameDto>>(games));
    }
}
