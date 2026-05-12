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
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAvatarItemHandler> _logger;
    private const string ContainerName = "avatar-items";

    public CreateAvatarItemHandler(
        IAvatarItemRepository repo,
        IBlobService blobService,
        IMapper mapper,
        ILogger<CreateAvatarItemHandler> logger)
    {
        _repo = repo;
        _blobService = blobService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<AvatarItemDto>> HandleAsync(CreateAvatarItemRequest request, CancellationToken ct = default)
    {
        var dto = request.Dto;
        _logger.LogInformation("Creating avatar item {Name}", dto.Name);
        var item = _mapper.Map<AvatarItem>(dto);

        if (dto.Image != null)
        {
            if (!IsValidImage(dto.Image))
            {
                _logger.LogWarning("Invalid image upload for avatar item {Name}", dto.Name);
                return HandlerResult<AvatarItemDto>.BadRequest("Invalid image. Only JPG, PNG and WebP files under 5MB are allowed.");
            }
            item.PreviewAssetUrl = await _blobService.UploadAsync(dto.Image, ContainerName);
            _logger.LogInformation("Image uploaded for avatar item {Name}: {Url}", dto.Name, item.PreviewAssetUrl);
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
}
