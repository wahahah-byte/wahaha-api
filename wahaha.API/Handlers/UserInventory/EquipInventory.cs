using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.UserInventory;

public sealed record EquipInventoryRequest(int InventoryId, Guid UserId);

public sealed class EquipInventoryHandler : IRequestHandler<EquipInventoryRequest, Unit>
{
    private readonly IUserInventoryRepository _repo;
    private readonly ILogger<EquipInventoryHandler> _logger;

    public EquipInventoryHandler(IUserInventoryRepository repo, ILogger<EquipInventoryHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(EquipInventoryRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Equipping inventory item {InventoryId}", request.InventoryId);
        var entry = await _repo.GetByIdAsync(request.InventoryId);
        if (entry == null || entry.UserId != request.UserId)
        {
            _logger.LogWarning("Inventory entry {InventoryId} not found or unauthorized for equip", request.InventoryId);
            return HandlerResult<Unit>.NotFound($"Inventory entry with ID {request.InventoryId} was not found.");
        }
        await _repo.EquipAsync(request.InventoryId);
        _logger.LogInformation("Inventory item {InventoryId} equipped successfully", request.InventoryId);
        return HandlerResult<Unit>.NoContent();
    }
}
