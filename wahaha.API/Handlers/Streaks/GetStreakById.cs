using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Streaks;

public sealed record GetStreakByIdRequest(int StreakId, Guid UserId);

public sealed class GetStreakByIdHandler : IRequestHandler<GetStreakByIdRequest, StreakDto>
{
    private readonly IStreakRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetStreakByIdHandler> _logger;

    public GetStreakByIdHandler(IStreakRepository repo, IMapper mapper, ILogger<GetStreakByIdHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<StreakDto>> HandleAsync(GetStreakByIdRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching streak {StreakId}", request.StreakId);
        var s = await _repo.GetByIdAsync(request.StreakId);
        if (s == null || s.UserId != request.UserId)
        {
            _logger.LogWarning("Streak {StreakId} not found or unauthorized", request.StreakId);
            return HandlerResult<StreakDto>.NotFound($"Streak with ID {request.StreakId} was not found.");
        }
        return HandlerResult<StreakDto>.Ok(_mapper.Map<StreakDto>(s));
    }
}
