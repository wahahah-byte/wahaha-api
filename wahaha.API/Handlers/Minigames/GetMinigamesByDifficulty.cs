using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Minigames;

public sealed record GetMinigamesByDifficultyRequest(Difficulty Difficulty);

public sealed class GetMinigamesByDifficultyHandler : IRequestHandler<GetMinigamesByDifficultyRequest, IEnumerable<MinigameDto>>
{
    private readonly IMinigameRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMinigamesByDifficultyHandler> _logger;

    public GetMinigamesByDifficultyHandler(IMinigameRepository repo, IMapper mapper, ILogger<GetMinigamesByDifficultyHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<MinigameDto>>> HandleAsync(GetMinigamesByDifficultyRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching minigames by difficulty {Difficulty}", request.Difficulty);
        var games = await _repo.GetByDifficultyAsync(request.Difficulty);
        return HandlerResult<IEnumerable<MinigameDto>>.Ok(_mapper.Map<IEnumerable<MinigameDto>>(games));
    }
}
