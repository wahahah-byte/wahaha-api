using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using DomainUserInventory = wahaha.API.Models.Domain.UserInventory;

namespace wahaha.API.Handlers.AvatarItems;

// The shop's purchase flow. Composes the four steps that have to succeed
// together for a clean buy: catalogue availability check, no-duplicate
// gate (matches the client's "hide owned items" UX — server enforces it
// too so a stale client can't double-buy), debit + transaction log, and
// inventory row creation with an optional auto-equip.
//
// Operations are sequenced read → write — not wrapped in an explicit DB
// transaction because the existing balance-mutation paths (SpendPoints /
// AddPoints / RefundPoints) don't either, and adopting transactions for
// this one handler would create an inconsistency with no upstream
// concurrency token. If we ever add row-version locking on Users, this
// handler should adopt it then.
public sealed record PurchaseAvatarItemRequest(Guid UserId, int ItemId, bool AutoEquip);

public sealed class PurchaseAvatarItemHandler
    : IRequestHandler<PurchaseAvatarItemRequest, PurchaseAvatarItemResponseDto>
{
    private readonly IAvatarItemRepository _itemRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUserInventoryRepository _inventoryRepo;
    private readonly IPointTransactionRepository _txRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<PurchaseAvatarItemHandler> _logger;

    public PurchaseAvatarItemHandler(
        IAvatarItemRepository itemRepo,
        IUserRepository userRepo,
        IUserInventoryRepository inventoryRepo,
        IPointTransactionRepository txRepo,
        IMapper mapper,
        ILogger<PurchaseAvatarItemHandler> logger)
    {
        _itemRepo = itemRepo;
        _userRepo = userRepo;
        _inventoryRepo = inventoryRepo;
        _txRepo = txRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<PurchaseAvatarItemResponseDto>> HandleAsync(
        PurchaseAvatarItemRequest request,
        CancellationToken ct = default)
    {
        if (request.UserId == Guid.Empty)
            return HandlerResult<PurchaseAvatarItemResponseDto>.BadRequest("No authenticated user.");

        var item = await _itemRepo.GetByIdAsync(request.ItemId);
        if (item == null)
            return HandlerResult<PurchaseAvatarItemResponseDto>.NotFound($"Avatar item with ID {request.ItemId} was not found.");
        if (!item.IsAvailable)
            return HandlerResult<PurchaseAvatarItemResponseDto>.BadRequest("This item isn't currently available for purchase.");

        if (await _inventoryRepo.UserOwnsItemAsync(request.UserId, request.ItemId))
            return HandlerResult<PurchaseAvatarItemResponseDto>.Conflict("You already own this item.");

        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user == null)
            return HandlerResult<PurchaseAvatarItemResponseDto>.NotFound("User not found.");
        if (user.CurrentBalance < item.Cost)
            return HandlerResult<PurchaseAvatarItemResponseDto>.BadRequest(
                $"Insufficient points: need {item.Cost}, have {user.CurrentBalance}.");

        // Log the transaction first, then debit — matches the order used by
        // CheckInTask.cs for the EARN side. If the debit fails after the tx
        // row is written we'd have an orphan SPEND row; we keep the order
        // because the inverse (debit-then-log) risks losing the audit trail
        // if the tx write throws after balance has already moved.
        var transaction = new PointTransaction
        {
            UserId = request.UserId,
            Amount = item.Cost,
            Type = TransactionType.SPEND,
            SourceType = SourceType.shop_item,
            SourceId = item.ItemId,
            Category = item.Category,
            Description = $"Bought {item.Name}",
            CreatedAt = DateTime.UtcNow,
        };
        await _txRepo.CreateAsync(transaction);

        var debited = await _userRepo.SpendPointsAsync(request.UserId, item.Cost);
        if (!debited)
        {
            _logger.LogError(
                "SpendPointsAsync returned false for user {UserId} buying item {ItemId} ({Cost} pts) after pre-check passed — possible race",
                request.UserId, request.ItemId, item.Cost);
            return HandlerResult<PurchaseAvatarItemResponseDto>.BadRequest("Could not debit points. Please try again.");
        }

        var inv = await _inventoryRepo.CreateAsync(new DomainUserInventory
        {
            UserId = request.UserId,
            ItemId = request.ItemId,
            IsEquipped = false,
            AcquiredAt = DateTime.UtcNow,
        });

        if (request.AutoEquip)
        {
            // EquipAsync unequips any currently-equipped item in the same
            // slot first so the chibi composite stays consistent.
            await _inventoryRepo.EquipAsync(inv.InventoryId);
            inv.IsEquipped = true;
        }

        // Re-load with AvatarItem nav populated (CreateAsync's return value
        // doesn't include it) so the client gets slot, asset URL, render
        // hints, bounds — everything the inventory grid + chibi need.
        var full = await _inventoryRepo.GetByIdAsync(inv.InventoryId) ?? inv;

        _logger.LogInformation(
            "User {UserId} purchased item {ItemId} ({Name}) for {Cost} pts (autoEquip={AutoEquip}); inventory {InventoryId}, new balance {Balance}",
            request.UserId, request.ItemId, item.Name, item.Cost, request.AutoEquip,
            inv.InventoryId, user.CurrentBalance - item.Cost);

        return HandlerResult<PurchaseAvatarItemResponseDto>.Ok(new PurchaseAvatarItemResponseDto
        {
            Inventory = _mapper.Map<UserInventoryDto>(full),
            NewBalance = user.CurrentBalance - item.Cost,
        });
    }
}
