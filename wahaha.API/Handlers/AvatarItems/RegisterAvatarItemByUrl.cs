using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;
using DomainUserInventory = wahaha.API.Models.Domain.UserInventory;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record RegisterAvatarItemByUrlRequest(Guid? UserId, RegisterAvatarItemByUrlDto Dto);

public sealed class RegisterAvatarItemByUrlHandler : IRequestHandler<RegisterAvatarItemByUrlRequest, AvatarItemDto>
{
    private readonly IAvatarItemRepository _avatarRepo;
    private readonly IUserInventoryRepository _inventoryRepo;
    private readonly IContentBoundsService _bounds;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<RegisterAvatarItemByUrlHandler> _logger;

    public RegisterAvatarItemByUrlHandler(
        IAvatarItemRepository avatarRepo,
        IUserInventoryRepository inventoryRepo,
        IContentBoundsService bounds,
        IHttpClientFactory httpClientFactory,
        IMapper mapper,
        ILogger<RegisterAvatarItemByUrlHandler> logger)
    {
        _avatarRepo = avatarRepo;
        _inventoryRepo = inventoryRepo;
        _bounds = bounds;
        _httpClientFactory = httpClientFactory;
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
            SecondaryAssetUrl = string.IsNullOrWhiteSpace(dto.SecondaryAssetUrl) ? null : dto.SecondaryAssetUrl,
            IsAvailable = dto.IsAvailable,
            // Pass through optional render hints. Null = use slot defaults
            // on the client.
            CoversHairFront = dto.CoversHairFront,
            CoversHairBack = dto.CoversHairBack,
            OffsetX = dto.OffsetX,
            OffsetY = dto.OffsetY,
            RenderScale = dto.RenderScale,
            SourceWidth = dto.SourceWidth,
            SourceHeight = dto.SourceHeight,
        };

        // Auto-compute the content bbox at register time so the inventory
        // card auto-centres without the admin having to click Recenter
        // afterwards. Only works for absolute http(s) URLs (the same shape
        // the recompute-bounds endpoint accepts); relative seed paths and
        // non-http schemes are skipped — the bounds stay null and the
        // client falls back to slot defaults until someone re-uploads or
        // Recenter is invoked. Failures are logged and swallowed: a bbox
        // miss should never block creating the row.
        var bounds = await TryComputeBoundsAsync(dto.PreviewAssetUrl, ct);
        if (bounds != null)
        {
            item.ContentMinX = bounds.MinX;
            item.ContentMinY = bounds.MinY;
            item.ContentMaxX = bounds.MaxX;
            item.ContentMaxY = bounds.MaxY;
        }

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

    // Fetch + scan-or-bail. Mirrors RecomputeAvatarItemBoundsHandler but
    // never surfaces an error to the caller — we don't want the register
    // call to fail because the asset's host is temporarily unreachable.
    private async Task<ContentBounds?> TryComputeBoundsAsync(string url, CancellationToken ct)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            _logger.LogInformation("Skipping bbox scan — {Url} is not an absolute http(s) URL", url);
            return null;
        }

        try
        {
            var http = _httpClientFactory.CreateClient();
            using var resp = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Skipping bbox scan — fetch {Url} returned {Status}", url, (int)resp.StatusCode);
                return null;
            }
            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            return await _bounds.ComputeAsync(stream, uri.ToString(), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Skipping bbox scan — fetch/parse failed for {Url}", url);
            return null;
        }
    }
}
