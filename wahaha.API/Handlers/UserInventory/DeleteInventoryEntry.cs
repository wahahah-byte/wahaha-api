using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.UserInventory;

public sealed record DeleteInventoryEntryRequest(int InventoryId, Guid UserId);

public sealed class DeleteInventoryEntryHandler : IRequestHandler<DeleteInventoryEntryRequest, Unit>
{
    private readonly IUserInventoryRepository _repo;
    private readonly ILogger<DeleteInventoryEntryHandler> _logger;

    public DeleteInventoryEntryHandler(IUserInventoryRepository repo, ILogger<DeleteInventoryEntryHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(DeleteInventoryEntryRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting inventory entry {InventoryId}", request.InventoryId);
        var entry = await _repo.GetByIdAsync(request.InventoryId);
        if (entry == null || entry.UserId != request.UserId)
        {
            _logger.LogWarning("Inventory entry {InventoryId} not found or unauthorized for deletion", request.InventoryId);
            return HandlerResult<Unit>.NotFound($"Inventory entry with ID {request.InventoryId} was not found.");
        }
        await _repo.DeleteAsync(request.InventoryId);
        _logger.LogInformation("Inventory entry {InventoryId} deleted successfully", request.InventoryId);
        return HandlerResult<Unit>.NoContent();
    }
}
