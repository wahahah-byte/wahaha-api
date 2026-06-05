using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.UserInventory;

public sealed record SellInventoryRequest(int InventoryId, Guid UserId);

// Sell-back: removes an inventory row and credits floor(item.Cost * SellRefundRatio); unequips first if equipped.
public sealed class SellInventoryHandler : IRequestHandler<SellInventoryRequest, SellInventoryResponseDto>
{
    // 50% refund — discourages buy/sell farming while keeping "regret" actions cheap.
    public const double SellRefundRatio = 0.5;

    private readonly IUserInventoryRepository _inventoryRepo;
    private readonly IUserRepository _userRepo;
    private readonly IPointTransactionRepository _txRepo;
    private readonly ILogger<SellInventoryHandler> _logger;

    public SellInventoryHandler(
        IUserInventoryRepository inventoryRepo,
        IUserRepository userRepo,
        IPointTransactionRepository txRepo,
        ILogger<SellInventoryHandler> logger)
    {
        _inventoryRepo = inventoryRepo;
        _userRepo = userRepo;
        _txRepo = txRepo;
        _logger = logger;
    }

    public async Task<HandlerResult<SellInventoryResponseDto>> HandleAsync(
        SellInventoryRequest request,
        CancellationToken ct = default)
    {
        if (request.UserId == Guid.Empty)
            return HandlerResult<SellInventoryResponseDto>.BadRequest("No authenticated user.");

        var entry = await _inventoryRepo.GetByIdAsync(request.InventoryId);
        if (entry == null || entry.UserId != request.UserId)
        {
            _logger.LogWarning("Inventory {InventoryId} not found or unauthorized for sell", request.InventoryId);
            return HandlerResult<SellInventoryResponseDto>.NotFound(
                $"Inventory entry with ID {request.InventoryId} was not found.");
        }
        if (entry.AvatarItem == null)
        {
            _logger.LogWarning("Inventory {InventoryId} has no AvatarItem nav populated — cannot price refund", request.InventoryId);
            return HandlerResult<SellInventoryResponseDto>.BadRequest("Item details unavailable for refund.");
        }

        var item = entry.AvatarItem;
        var refund = (int)Math.Floor(item.Cost * SellRefundRatio);

        // Auto-unequip before deleting so the chibi doesn't render a stale slot reference.
        if (entry.IsEquipped) await _inventoryRepo.UnequipAsync(entry.InventoryId);

        // Audit-first ordering matches PurchaseAvatarItem: tx logged, then balance moved, then row removed.
        if (refund > 0)
        {
            var tx = new PointTransaction
            {
                UserId = request.UserId,
                Amount = refund,
                Type = TransactionType.EARN,
                SourceType = SourceType.shop_item,
                SourceId = item.ItemId,
                Category = item.Category,
                Description = $"Sold {item.Name}",
                CreatedAt = DateTime.UtcNow,
            };
            await _txRepo.CreateAsync(tx);
            var credited = await _userRepo.AddPointsAsync(request.UserId, refund);
            if (!credited)
            {
                _logger.LogError(
                    "AddPointsAsync returned false for user {UserId} selling inventory {InventoryId} (refund {Refund})",
                    request.UserId, request.InventoryId, refund);
                return HandlerResult<SellInventoryResponseDto>.BadRequest("Could not credit refund. Please try again.");
            }
        }

        var deleted = await _inventoryRepo.DeleteAsync(entry.InventoryId);
        if (!deleted)
        {
            _logger.LogError("Inventory {InventoryId} delete returned false after refund credited", request.InventoryId);
            // Refund was already credited — surface the (now-orphaned) state as an error.
            return HandlerResult<SellInventoryResponseDto>.BadRequest("Refund applied but inventory removal failed.");
        }

        // Reload balance for an authoritative post-sale figure.
        var user = await _userRepo.GetByIdAsync(request.UserId);
        var newBalance = user?.CurrentBalance ?? 0;

        _logger.LogInformation(
            "User {UserId} sold inventory {InventoryId} (item {ItemId} '{Name}') for {Refund} pts; new balance {Balance}",
            request.UserId, request.InventoryId, item.ItemId, item.Name, refund, newBalance);

        return HandlerResult<SellInventoryResponseDto>.Ok(new SellInventoryResponseDto
        {
            InventoryId = entry.InventoryId,
            RefundedPoints = refund,
            NewBalance = newBalance,
        });
    }
}
