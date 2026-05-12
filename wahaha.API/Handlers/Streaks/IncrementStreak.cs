using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Streaks;

public sealed record IncrementStreakRequest(int StreakId, Guid UserId);

public sealed class IncrementStreakHandler : IRequestHandler<IncrementStreakRequest, Unit>
{
    private readonly IStreakRepository _repo;
    private readonly ILogger<IncrementStreakHandler> _logger;

    public IncrementStreakHandler(IStreakRepository repo, ILogger<IncrementStreakHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(IncrementStreakRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Incrementing streak {StreakId}", request.StreakId);
        var streak = await _repo.GetByIdAsync(request.StreakId);
        if (streak == null || streak.UserId != request.UserId)
        {
            _logger.LogWarning("Streak {StreakId} not found or unauthorized for increment", request.StreakId);
            return HandlerResult<Unit>.NotFound($"Streak with ID {request.StreakId} was not found.");
        }
        await _repo.IncrementAsync(request.StreakId);
        return HandlerResult<Unit>.NoContent();
    }
}
