using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Users;

public sealed record SpendPointsRequest(Guid UserId, int Points);

public sealed class SpendPointsHandler : IRequestHandler<SpendPointsRequest, Unit>
{
    private readonly IUserRepository _repo;
    private readonly ILogger<SpendPointsHandler> _logger;

    public SpendPointsHandler(IUserRepository repo, ILogger<SpendPointsHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(SpendPointsRequest request, CancellationToken ct = default)
    {
        if (request.Points <= 0)
            return HandlerResult<Unit>.BadRequest("Points must be a positive number.");

        _logger.LogInformation("Spending {Points} points for user {UserId}", request.Points, request.UserId);
        var success = await _repo.SpendPointsAsync(request.UserId, request.Points);
        if (!success)
        {
            _logger.LogWarning("User {UserId} not found or insufficient balance when spending {Points} points",
                request.UserId, request.Points);
            return HandlerResult<Unit>.NotFound("User was not found or has insufficient balance.");
        }
        return HandlerResult<Unit>.NoContent();
    }
}
