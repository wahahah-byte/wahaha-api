using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Minigames;

public sealed record DeleteMinigameRequest(int GameId);

public sealed class DeleteMinigameHandler : IRequestHandler<DeleteMinigameRequest, Unit>
{
    private readonly IMinigameRepository _repo;
    private readonly ILogger<DeleteMinigameHandler> _logger;

    public DeleteMinigameHandler(IMinigameRepository repo, ILogger<DeleteMinigameHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(DeleteMinigameRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting minigame {GameId}", request.GameId);
        var success = await _repo.DeleteAsync(request.GameId);
        if (!success)
        {
            _logger.LogWarning("Minigame {GameId} not found for deletion", request.GameId);
            return HandlerResult<Unit>.NotFound($"Minigame with ID {request.GameId} was not found.");
        }
        _logger.LogInformation("Minigame {GameId} deleted successfully", request.GameId);
        return HandlerResult<Unit>.NoContent();
    }
}
