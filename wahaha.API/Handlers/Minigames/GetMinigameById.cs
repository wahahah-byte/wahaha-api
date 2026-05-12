using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Minigames;

public sealed record GetMinigameByIdRequest(int GameId);

public sealed class GetMinigameByIdHandler : IRequestHandler<GetMinigameByIdRequest, MinigameDto>
{
    private readonly IMinigameRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMinigameByIdHandler> _logger;

    public GetMinigameByIdHandler(IMinigameRepository repo, IMapper mapper, ILogger<GetMinigameByIdHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<MinigameDto>> HandleAsync(GetMinigameByIdRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching minigame {GameId}", request.GameId);
        var game = await _repo.GetByIdAsync(request.GameId);
        if (game == null)
        {
            _logger.LogWarning("Minigame {GameId} not found", request.GameId);
            return HandlerResult<MinigameDto>.NotFound($"Minigame with ID {request.GameId} was not found.");
        }
        return HandlerResult<MinigameDto>.Ok(_mapper.Map<MinigameDto>(game));
    }
}
