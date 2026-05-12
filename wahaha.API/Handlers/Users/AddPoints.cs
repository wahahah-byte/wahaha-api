using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Users;

public sealed record AddPointsRequest(Guid UserId, int Points);

public sealed class AddPointsHandler : IRequestHandler<AddPointsRequest, Unit>
{
    private readonly IUserRepository _repo;
    private readonly ILogger<AddPointsHandler> _logger;

    public AddPointsHandler(IUserRepository repo, ILogger<AddPointsHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(AddPointsRequest request, CancellationToken ct = default)
    {
        if (request.Points <= 0)
            return HandlerResult<Unit>.BadRequest("Points must be a positive number.");

        _logger.LogInformation("Adding {Points} points to user {UserId}", request.Points, request.UserId);
        var success = await _repo.AddPointsAsync(request.UserId, request.Points);
        if (!success)
        {
            _logger.LogWarning("User {UserId} not found when adding points", request.UserId);
            return HandlerResult<Unit>.NotFound("User was not found.");
        }
        return HandlerResult<Unit>.NoContent();
    }
}
