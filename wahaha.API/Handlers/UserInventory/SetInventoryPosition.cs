using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.UserInventory;

public sealed record SetInventoryPositionRequest(int InventoryId, Guid UserId, UpdateInventoryPositionDto Dto);

public sealed class SetInventoryPositionHandler : IRequestHandler<SetInventoryPositionRequest, Unit>
{
    private readonly IUserInventoryRepository _repo;
    private readonly ILogger<SetInventoryPositionHandler> _logger;

    public SetInventoryPositionHandler(IUserInventoryRepository repo, ILogger<SetInventoryPositionHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(SetInventoryPositionRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Setting position for inventory item {InventoryId} to ({X}, {Y})",
            request.InventoryId, request.Dto.PositionX, request.Dto.PositionY);

        var entry = await _repo.GetByIdAsync(request.InventoryId);
        if (entry == null || entry.UserId != request.UserId)
        {
            _logger.LogWarning("Inventory entry {InventoryId} not found or unauthorized for position update", request.InventoryId);
            return HandlerResult<Unit>.NotFound($"Inventory entry with ID {request.InventoryId} was not found.");
        }

        entry.PositionX = request.Dto.PositionX;
        entry.PositionY = request.Dto.PositionY;
        // Only overwrite rotation when the caller explicitly sends one — keeps
        // older position-only clients backward compatible.
        if (request.Dto.IsRotated.HasValue) entry.IsRotated = request.Dto.IsRotated.Value;
        await _repo.UpdateAsync(entry);
        return HandlerResult<Unit>.NoContent();
    }
}
