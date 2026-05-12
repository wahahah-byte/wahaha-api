using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record DeleteAvatarItemRequest(int ItemId);

public sealed class DeleteAvatarItemHandler : IRequestHandler<DeleteAvatarItemRequest, Unit>
{
    private readonly IAvatarItemRepository _repo;
    private readonly IBlobService _blobService;
    private readonly ILogger<DeleteAvatarItemHandler> _logger;
    private const string ContainerName = "avatar-items";

    public DeleteAvatarItemHandler(IAvatarItemRepository repo, IBlobService blobService, ILogger<DeleteAvatarItemHandler> logger)
    {
        _repo = repo;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(DeleteAvatarItemRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting avatar item {ItemId}", request.ItemId);
        var item = await _repo.GetByIdAsync(request.ItemId);
        if (item == null)
        {
            _logger.LogWarning("Avatar item {ItemId} not found for deletion", request.ItemId);
            return HandlerResult<Unit>.NotFound($"Avatar item with ID {request.ItemId} was not found.");
        }
        if (!string.IsNullOrEmpty(item.PreviewAssetUrl))
            await _blobService.DeleteAsync(item.PreviewAssetUrl, ContainerName);
        await _repo.DeleteAsync(request.ItemId);
        _logger.LogInformation("Avatar item {ItemId} deleted successfully", request.ItemId);
        return HandlerResult<Unit>.NoContent();
    }
}
