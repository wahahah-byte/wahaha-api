using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.MinigameSessions;

public sealed record GetSessionListRequest(Guid UserId, MinigameSessionFilterParams Filters);

public sealed class GetSessionListHandler : IRequestHandler<GetSessionListRequest, IEnumerable<MinigameSessionDto>>
{
    private readonly IMinigameSessionRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSessionListHandler> _logger;

    public GetSessionListHandler(IMinigameSessionRepository repo, IMapper mapper, ILogger<GetSessionListHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<MinigameSessionDto>>> HandleAsync(GetSessionListRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching sessions for user {UserId}", request.UserId);
        request.Filters.UserId = request.UserId;
        var sessions = await _repo.GetFilteredAsync(request.Filters);
        return HandlerResult<IEnumerable<MinigameSessionDto>>.Ok(_mapper.Map<IEnumerable<MinigameSessionDto>>(sessions));
    }
}
