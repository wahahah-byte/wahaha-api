using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Streaks;

public sealed record DeleteStreakRequest(int StreakId, Guid UserId);

public sealed class DeleteStreakHandler : IRequestHandler<DeleteStreakRequest, Unit>
{
    private readonly IStreakRepository _repo;
    private readonly ILogger<DeleteStreakHandler> _logger;

    public DeleteStreakHandler(IStreakRepository repo, ILogger<DeleteStreakHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(DeleteStreakRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting streak {StreakId}", request.StreakId);
        var streak = await _repo.GetByIdAsync(request.StreakId);
        if (streak == null || streak.UserId != request.UserId)
        {
            _logger.LogWarning("Streak {StreakId} not found or unauthorized for deletion", request.StreakId);
            return HandlerResult<Unit>.NotFound($"Streak with ID {request.StreakId} was not found.");
        }
        await _repo.DeleteAsync(request.StreakId);
        _logger.LogInformation("Streak {StreakId} deleted successfully", request.StreakId);
        return HandlerResult<Unit>.NoContent();
    }
}
