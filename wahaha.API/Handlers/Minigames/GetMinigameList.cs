using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Minigames;

public sealed record GetMinigameListRequest(MinigameFilterParams Filters);

public sealed class GetMinigameListHandler : IRequestHandler<GetMinigameListRequest, IEnumerable<MinigameDto>>
{
    private readonly IMinigameRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMinigameListHandler> _logger;

    public GetMinigameListHandler(IMinigameRepository repo, IMapper mapper, ILogger<GetMinigameListHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<MinigameDto>>> HandleAsync(GetMinigameListRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching minigames with filters");
        var games = await _repo.GetFilteredAsync(request.Filters);
        return HandlerResult<IEnumerable<MinigameDto>>.Ok(_mapper.Map<IEnumerable<MinigameDto>>(games));
    }
}
