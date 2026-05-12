using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Streaks;

public sealed record ResetStreakRequest(int StreakId, Guid UserId);

public sealed class ResetStreakHandler : IRequestHandler<ResetStreakRequest, Unit>
{
    private readonly IStreakRepository _repo;
    private readonly ILogger<ResetStreakHandler> _logger;

    public ResetStreakHandler(IStreakRepository repo, ILogger<ResetStreakHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(ResetStreakRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Resetting streak {StreakId}", request.StreakId);
        var streak = await _repo.GetByIdAsync(request.StreakId);
        if (streak == null || streak.UserId != request.UserId)
        {
            _logger.LogWarning("Streak {StreakId} not found or unauthorized for reset", request.StreakId);
            return HandlerResult<Unit>.NotFound($"Streak with ID {request.StreakId} was not found.");
        }
        await _repo.ResetAsync(request.StreakId);
        return HandlerResult<Unit>.NoContent();
    }
}
