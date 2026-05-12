using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Streaks;

public sealed record GetActiveStreaksRequest(Guid UserId);

public sealed class GetActiveStreaksHandler : IRequestHandler<GetActiveStreaksRequest, IEnumerable<StreakDto>>
{
    private readonly IStreakRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetActiveStreaksHandler> _logger;

    public GetActiveStreaksHandler(IStreakRepository repo, IMapper mapper, ILogger<GetActiveStreaksHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<StreakDto>>> HandleAsync(GetActiveStreaksRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching active streaks for user {UserId}", request.UserId);
        var streaks = await _repo.GetActiveByUserAsync(request.UserId);
        return HandlerResult<IEnumerable<StreakDto>>.Ok(_mapper.Map<IEnumerable<StreakDto>>(streaks));
    }
}
