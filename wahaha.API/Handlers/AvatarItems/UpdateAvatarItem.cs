using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record UpdateAvatarItemRequest(int ItemId, UpdateAvatarItemDto Dto);

public sealed class UpdateAvatarItemHandler : IRequestHandler<UpdateAvatarItemRequest, Unit>
{
    private readonly IAvatarItemRepository _repo;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateAvatarItemHandler> _logger;
    private const string ContainerName = "avatar-items";

    public UpdateAvatarItemHandler(
        IAvatarItemRepository repo,
        IBlobService blobService,
        IMapper mapper,
        ILogger<UpdateAvatarItemHandler> logger)
    {
        _repo = repo;
        _blobService = blobService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(UpdateAvatarItemRequest request, CancellationToken ct = default)
    {
        if (request.ItemId != request.Dto.ItemId)
            return HandlerResult<Unit>.BadRequest("Item ID in the URL does not match the request body.");

        _logger.LogInformation("Updating avatar item {ItemId}", request.ItemId);
        var item = await _repo.GetByIdAsync(request.ItemId);
        if (item == null)
        {
            _logger.LogWarning("Avatar item {ItemId} not found for update", request.ItemId);
            return HandlerResult<Unit>.NotFound($"Avatar item with ID {request.ItemId} was not found.");
        }

        if (request.Dto.Image != null)
        {
            if (!IsValidImage(request.Dto.Image))
            {
                _logger.LogWarning("Invalid image upload for avatar item {ItemId}", request.ItemId);
                return HandlerResult<Unit>.BadRequest("Invalid image. Only JPG, PNG and WebP files under 5MB are allowed.");
            }
            if (!string.IsNullOrEmpty(item.PreviewAssetUrl))
                await _blobService.DeleteAsync(item.PreviewAssetUrl, ContainerName);
            item.PreviewAssetUrl = await _blobService.UploadAsync(request.Dto.Image, ContainerName);
            _logger.LogInformation("Image updated for avatar item {ItemId}: {Url}", request.ItemId, item.PreviewAssetUrl);
        }

        _mapper.Map(request.Dto, item);
        await _repo.UpdateAsync(item);
        _logger.LogInformation("Avatar item {ItemId} updated successfully", request.ItemId);
        return HandlerResult<Unit>.NoContent();
    }

    private static bool IsValidImage(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        var maxSize = 5 * 1024 * 1024;
        return allowedTypes.Contains(file.ContentType.ToLower()) && file.Length <= maxSize;
    }
}
