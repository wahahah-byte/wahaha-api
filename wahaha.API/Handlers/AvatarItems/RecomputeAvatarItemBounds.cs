using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record RecomputeAvatarItemBoundsRequest(int ItemId);

// Re-scan PreviewAssetUrl bytes and update content bounds; absolute http(s) URLs only.
public sealed class RecomputeAvatarItemBoundsHandler
    : IRequestHandler<RecomputeAvatarItemBoundsRequest, AvatarItemDto>
{
    private readonly IAvatarItemRepository _repo;
    private readonly IContentBoundsService _bounds;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<RecomputeAvatarItemBoundsHandler> _logger;

    public RecomputeAvatarItemBoundsHandler(
        IAvatarItemRepository repo,
        IContentBoundsService bounds,
        IHttpClientFactory httpClientFactory,
        IMapper mapper,
        ILogger<RecomputeAvatarItemBoundsHandler> logger)
    {
        _repo = repo;
        _bounds = bounds;
        _httpClientFactory = httpClientFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<AvatarItemDto>> HandleAsync(
        RecomputeAvatarItemBoundsRequest request,
        CancellationToken ct = default)
    {
        var item = await _repo.GetByIdAsync(request.ItemId);
        if (item == null)
            return HandlerResult<AvatarItemDto>.NotFound($"Avatar item with ID {request.ItemId} was not found.");

        if (string.IsNullOrWhiteSpace(item.PreviewAssetUrl))
            return HandlerResult<AvatarItemDto>.BadRequest("Item has no PreviewAssetUrl to scan.");

        if (!Uri.TryCreate(item.PreviewAssetUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return HandlerResult<AvatarItemDto>.BadRequest(
                "PreviewAssetUrl is not an absolute http(s) URL — edit the item and re-upload the PNG instead.");
        }

        _logger.LogInformation("Recomputing bounds for item {ItemId} from {Url}", request.ItemId, uri);

        var http = _httpClientFactory.CreateClient();
        // Stream response directly into ImageSharp to skip the extra byte[] copy.
        using var resp = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Recompute fetch failed for item {ItemId}: {Status} from {Url}",
                request.ItemId, (int)resp.StatusCode, uri);
            return HandlerResult<AvatarItemDto>.BadRequest($"Failed to fetch image ({(int)resp.StatusCode}).");
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var bounds = await _bounds.ComputeAsync(stream, uri.ToString(), ct);

        if (bounds != null)
        {
            item.ContentMinX = bounds.MinX;
            item.ContentMinY = bounds.MinY;
            item.ContentMaxX = bounds.MaxX;
            item.ContentMaxY = bounds.MaxY;
        }
        else
        {
            // Unscannable image; clear bounds so client falls back to slot defaults.
            item.ContentMinX = null;
            item.ContentMinY = null;
            item.ContentMaxX = null;
            item.ContentMaxY = null;
        }

        await _repo.UpdateAsync(item);
        _logger.LogInformation("Recomputed bounds for item {ItemId}: {Bounds}",
            request.ItemId, bounds?.ToString() ?? "<none>");

        return HandlerResult<AvatarItemDto>.Ok(_mapper.Map<AvatarItemDto>(item));
    }
}
