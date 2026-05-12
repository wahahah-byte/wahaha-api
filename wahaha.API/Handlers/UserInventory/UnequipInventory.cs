using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.UserInventory;

public sealed record UnequipInventoryRequest(int InventoryId, Guid UserId);

public sealed class UnequipInventoryHandler : IRequestHandler<UnequipInventoryRequest, Unit>
{
    private readonly IUserInventoryRepository _repo;
    private readonly ILogger<UnequipInventoryHandler> _logger;

    public UnequipInventoryHandler(IUserInventoryRepository repo, ILogger<UnequipInventoryHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(UnequipInventoryRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Unequipping inventory item {InventoryId}", request.InventoryId);
        var entry = await _repo.GetByIdAsync(request.InventoryId);
        if (entry == null || entry.UserId != request.UserId)
        {
            _logger.LogWarning("Inventory entry {InventoryId} not found or unauthorized for unequip", request.InventoryId);
            return HandlerResult<Unit>.NotFound($"Inventory entry with ID {request.InventoryId} was not found.");
        }
        await _repo.UnequipAsync(request.InventoryId);
        _logger.LogInformation("Inventory item {InventoryId} unequipped successfully", request.InventoryId);
        return HandlerResult<Unit>.NoContent();
    }
}
