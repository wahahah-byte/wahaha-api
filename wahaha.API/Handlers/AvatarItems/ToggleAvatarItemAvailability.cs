using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record ToggleAvatarItemAvailabilityRequest(int ItemId);

public sealed class ToggleAvatarItemAvailabilityHandler : IRequestHandler<ToggleAvatarItemAvailabilityRequest, Unit>
{
    private readonly IAvatarItemRepository _repo;
    private readonly ILogger<ToggleAvatarItemAvailabilityHandler> _logger;

    public ToggleAvatarItemAvailabilityHandler(IAvatarItemRepository repo, ILogger<ToggleAvatarItemAvailabilityHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(ToggleAvatarItemAvailabilityRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Toggling availability for avatar item {ItemId}", request.ItemId);
        var success = await _repo.ToggleAvailabilityAsync(request.ItemId);
        if (!success)
        {
            _logger.LogWarning("Avatar item {ItemId} not found for toggle", request.ItemId);
            return HandlerResult<Unit>.NotFound($"Avatar item with ID {request.ItemId} was not found.");
        }
        return HandlerResult<Unit>.NoContent();
    }
}
