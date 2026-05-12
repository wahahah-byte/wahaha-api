using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using DomainUserInventory = wahaha.API.Models.Domain.UserInventory;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record RegisterAvatarItemByUrlRequest(Guid? UserId, RegisterAvatarItemByUrlDto Dto);

public sealed class RegisterAvatarItemByUrlHandler : IRequestHandler<RegisterAvatarItemByUrlRequest, AvatarItemDto>
{
    private readonly IAvatarItemRepository _avatarRepo;
    private readonly IUserInventoryRepository _inventoryRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<RegisterAvatarItemByUrlHandler> _logger;

    public RegisterAvatarItemByUrlHandler(
        IAvatarItemRepository avatarRepo,
        IUserInventoryRepository inventoryRepo,
        IMapper mapper,
        ILogger<RegisterAvatarItemByUrlHandler> logger)
    {
        _avatarRepo = avatarRepo;
        _inventoryRepo = inventoryRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<AvatarItemDto>> HandleAsync(RegisterAvatarItemByUrlRequest request, CancellationToken ct = default)
    {
        var dto = request.Dto;
        if (!Enum.TryParse<ItemSlot>(dto.Slot, true, out var slot))
            return HandlerResult<AvatarItemDto>.BadRequest($"Invalid slot. Valid: {string.Join(", ", Enum.GetNames<ItemSlot>())}");
        if (!Enum.TryParse<Rarity>(dto.Rarity, true, out var rarity))
            return HandlerResult<AvatarItemDto>.BadRequest($"Invalid rarity. Valid: {string.Join(", ", Enum.GetNames<Rarity>())}");

        _logger.LogInformation("Registering avatar item {Name} from URL {Url}", dto.Name, dto.PreviewAssetUrl);

        var item = new AvatarItem
        {
            Name = dto.Name,
            Category = dto.Category,
            Slot = slot,
            Rarity = rarity,
            Cost = dto.Cost,
            Description = dto.Description,
            PreviewAssetUrl = dto.PreviewAssetUrl,
            IsAvailable = dto.IsAvailable,
        };
        var created = await _avatarRepo.CreateAsync(item);

        if (dto.GrantAndEquipForCurrentUser)
        {
            if (!request.UserId.HasValue || request.UserId.Value == Guid.Empty)
                return HandlerResult<AvatarItemDto>.BadRequest("No authenticated user — cannot grant.");

            var inv = await _inventoryRepo.CreateAsync(new DomainUserInventory
            {
                UserId = request.UserId.Value,
                ItemId = created.ItemId,
                IsEquipped = false,
                AcquiredAt = DateTime.UtcNow,
            });
            await _inventoryRepo.EquipAsync(inv.InventoryId);
            _logger.LogInformation("Granted item {ItemId} to user {UserId} (inventory {InventoryId}) and equipped",
                created.ItemId, request.UserId.Value, inv.InventoryId);
        }

        return HandlerResult<AvatarItemDto>.Ok(_mapper.Map<AvatarItemDto>(created));
    }
}
