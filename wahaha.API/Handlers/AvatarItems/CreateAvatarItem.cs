using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record CreateAvatarItemRequest(CreateAvatarItemDto Dto);

public sealed class CreateAvatarItemHandler : IRequestHandler<CreateAvatarItemRequest, AvatarItemDto>
{
    private readonly IAvatarItemRepository _repo;
    private readonly IBlobService _blobService;
    private readonly IContentBoundsService _boundsService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAvatarItemHandler> _logger;
    private const string ContainerName = "avatar-items";

    public CreateAvatarItemHandler(
        IAvatarItemRepository repo,
        IBlobService blobService,
        IContentBoundsService boundsService,
        IMapper mapper,
        ILogger<CreateAvatarItemHandler> logger)
    {
        _repo = repo;
        _blobService = blobService;
        _boundsService = boundsService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<AvatarItemDto>> HandleAsync(CreateAvatarItemRequest request, CancellationToken ct = default)
    {
        var dto = request.Dto;
        _logger.LogInformation("Creating avatar item {Name}", dto.Name);
        if (!Enum.TryParse<ItemSlot>(dto.Slot, true, out var slotEnum))
            return HandlerResult<AvatarItemDto>.BadRequest($"Invalid slot '{dto.Slot}'.");
        if (!AvatarCategoryWhitelist.IsValid(slotEnum, dto.Category))
            return HandlerResult<AvatarItemDto>.BadRequest(
                $"Invalid category '{dto.Category}' for slot {slotEnum}. Allowed: {AvatarCategoryWhitelist.AllowedFor(slotEnum)}.");
        var item = _mapper.Map<AvatarItem>(dto);

        if (dto.Image != null)
        {
            if (!IsValidImage(dto.Image))
            {
                _logger.LogWarning("Invalid image upload for avatar item {Name}", dto.Name);
                return HandlerResult<AvatarItemDto>.BadRequest("Invalid image. Only JPG, PNG and WebP files under 5MB are allowed.");
            }
            // Compute bbox first; IFormFile.OpenReadStream gives a fresh stream for the upload.
            var bounds = await _boundsService.ComputeAsync(dto.Image, ct);
            if (bounds != null)
            {
                item.ContentMinX = bounds.MinX;
                item.ContentMinY = bounds.MinY;
                item.ContentMaxX = bounds.MaxX;
                item.ContentMaxY = bounds.MaxY;
            }
            // Predictable blob name "{slot}_{slug(name)}.png"; BlobService appends "-2" etc on collision.
            item.PreviewAssetUrl = await _blobService.UploadAsync(
                dto.Image,
                ContainerName,
                BuildBlobName(dto.Slot, dto.Name));
            _logger.LogInformation("Image uploaded for avatar item {Name}: {Url}", dto.Name, item.PreviewAssetUrl);
        }

        // Secondary image: separate blob in same container; no bbox scan.
        if (dto.SecondaryImage != null)
        {
            if (!IsValidImage(dto.SecondaryImage))
            {
                _logger.LogWarning("Invalid secondary image upload for avatar item {Name}", dto.Name);
                return HandlerResult<AvatarItemDto>.BadRequest("Invalid secondary image. Only JPG, PNG and WebP files under 5MB are allowed.");
            }
            item.SecondaryAssetUrl = await _blobService.UploadAsync(
                dto.SecondaryImage,
                ContainerName,
                BuildBlobName(dto.Slot, dto.Name) + SecondarySuffix(dto.Slot));
            _logger.LogInformation("Secondary image uploaded for avatar item {Name}: {Url}", dto.Name, item.SecondaryAssetUrl);
        }

        // Equipped/worn image: separate blob; no bbox scan (only catalog preview is bbox-scanned).
        if (dto.EquippedImage != null)
        {
            if (!IsValidImage(dto.EquippedImage))
            {
                _logger.LogWarning("Invalid equipped image upload for avatar item {Name}", dto.Name);
                return HandlerResult<AvatarItemDto>.BadRequest("Invalid equipped image. Only JPG, PNG and WebP files under 5MB are allowed.");
            }
            item.EquippedAssetUrl = await _blobService.UploadAsync(
                dto.EquippedImage,
                ContainerName,
                BuildBlobName(dto.Slot, dto.Name) + "_equipped");
            _logger.LogInformation("Equipped image uploaded for avatar item {Name}: {Url}", dto.Name, item.EquippedAssetUrl);
        }

        var created = await _repo.CreateAsync(item);
        _logger.LogInformation("Avatar item {ItemId} ({Name}) created successfully", created.ItemId, created.Name);
        return HandlerResult<AvatarItemDto>.Ok(_mapper.Map<AvatarItemDto>(created));
    }

    private static bool IsValidImage(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        var maxSize = 5 * 1024 * 1024;
        return allowedTypes.Contains(file.ContentType.ToLower()) && file.Length <= maxSize;
    }

    // BlobService slugifies downstream so input only needs to be human-readable.
    private static string BuildBlobName(string slot, string name)
        => $"{slot}_{name}";

    // Slot-dependent secondary suffix: CAPE / OFFHAND secondaries are the FRONT overlay
    // (front drape / strap wrap over the arm); other slots use the secondary as a BACK layer.
    private static string SecondarySuffix(string slot)
        => slot.Equals("CAPE", StringComparison.OrdinalIgnoreCase)
           || slot.Equals("OFFHAND", StringComparison.OrdinalIgnoreCase)
            ? "_front" : "_back";
}
