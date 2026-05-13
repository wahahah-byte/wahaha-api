using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using DomainUserInventory = wahaha.API.Models.Domain.UserInventory;

namespace wahaha.API.Handlers.AvatarItems;

// Admin-only convenience for granting a catalogue item to any user without
// going through the regular acquire flow (which charges points and is
// gated by self). When TargetEmail is null/empty the item is granted to
// CallerUserId (= "grant to self"). AutoEquip flips the new inventory row
// to equipped after insert — same behaviour as the existing
// register-by-url GrantAndEquipForCurrentUser path.
public sealed record GrantAvatarItemRequest(
    Guid CallerUserId,
    int ItemId,
    string? TargetEmail,
    bool AutoEquip);

public sealed class GrantAvatarItemHandler
    : IRequestHandler<GrantAvatarItemRequest, UserInventoryDto>
{
    private readonly IAvatarItemRepository _itemRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUserInventoryRepository _inventoryRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<GrantAvatarItemHandler> _logger;

    public GrantAvatarItemHandler(
        IAvatarItemRepository itemRepo,
        IUserRepository userRepo,
        IUserInventoryRepository inventoryRepo,
        IMapper mapper,
        ILogger<GrantAvatarItemHandler> logger)
    {
        _itemRepo = itemRepo;
        _userRepo = userRepo;
        _inventoryRepo = inventoryRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<UserInventoryDto>> HandleAsync(
        GrantAvatarItemRequest request,
        CancellationToken ct = default)
    {
        var item = await _itemRepo.GetByIdAsync(request.ItemId);
        if (item == null)
            return HandlerResult<UserInventoryDto>.NotFound($"Avatar item with ID {request.ItemId} was not found.");

        // Resolve target — empty / whitespace email falls back to the caller.
        Guid targetUserId;
        if (string.IsNullOrWhiteSpace(request.TargetEmail))
        {
            if (request.CallerUserId == Guid.Empty)
                return HandlerResult<UserInventoryDto>.BadRequest("No authenticated user — cannot grant to self.");
            targetUserId = request.CallerUserId;
        }
        else
        {
            var target = await _userRepo.GetByEmailAsync(request.TargetEmail.Trim());
            if (target == null)
                return HandlerResult<UserInventoryDto>.NotFound($"No user with email '{request.TargetEmail}'.");
            targetUserId = target.UserId;
        }

        _logger.LogInformation("Admin granting item {ItemId} ({Name}) to user {UserId} (autoEquip={AutoEquip})",
            request.ItemId, item.Name, targetUserId, request.AutoEquip);

        // Duplicate grants are allowed — admins sometimes want to give a
        // user multiple copies of the same item (or undo a wrong unequip
        // by issuing a fresh row). No uniqueness check here on purpose.
        var inv = await _inventoryRepo.CreateAsync(new DomainUserInventory
        {
            UserId = targetUserId,
            ItemId = request.ItemId,
            IsEquipped = false,
            AcquiredAt = DateTime.UtcNow,
        });

        if (request.AutoEquip)
        {
            // EquipAsync unequips any currently-equipped item in the same
            // slot before flipping this row to equipped, so the chibi
            // composite stays consistent.
            await _inventoryRepo.EquipAsync(inv.InventoryId);
            inv.IsEquipped = true;
        }

        // Re-load with the AvatarItem nav populated so the response DTO
        // carries everything the inventory grid needs (slot, asset URL,
        // bounds, render hints). The CreateAsync return value lacks the
        // include — fetch via the byId path that does include it.
        var full = await _inventoryRepo.GetByIdAsync(inv.InventoryId) ?? inv;
        _logger.LogInformation("Granted item {ItemId} to user {UserId} as inventory {InventoryId}",
            request.ItemId, targetUserId, inv.InventoryId);

        return HandlerResult<UserInventoryDto>.Ok(_mapper.Map<UserInventoryDto>(full));
    }
}
