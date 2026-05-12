using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.MinigameSessions;

public sealed record DeleteSessionRequest(int SessionId, Guid UserId);

public sealed class DeleteSessionHandler : IRequestHandler<DeleteSessionRequest, Unit>
{
    private readonly IMinigameSessionRepository _repo;
    private readonly ILogger<DeleteSessionHandler> _logger;

    public DeleteSessionHandler(IMinigameSessionRepository repo, ILogger<DeleteSessionHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(DeleteSessionRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting session {SessionId}", request.SessionId);
        var session = await _repo.GetByIdAsync(request.SessionId);
        if (session == null || session.UserId != request.UserId)
        {
            _logger.LogWarning("Session {SessionId} not found or unauthorized for deletion", request.SessionId);
            return HandlerResult<Unit>.NotFound($"Session with ID {request.SessionId} was not found.");
        }
        await _repo.DeleteAsync(request.SessionId);
        _logger.LogInformation("Session {SessionId} deleted successfully", request.SessionId);
        return HandlerResult<Unit>.NoContent();
    }
}
