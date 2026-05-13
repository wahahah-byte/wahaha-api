using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record RecomputeAvatarItemBoundsRequest(int ItemId);

// Re-scans the bytes at an item's PreviewAssetUrl and updates the
// content_min/max_x/y columns. Useful for rows that pre-date the bbox
// feature, were registered via URL (which never computes bounds at create
// time), or got their PreviewAssetUrl reassigned out-of-band. Returns the
// updated DTO so the caller can splice it into its local catalogue without
// re-fetching the whole list.
//
// Relative URLs (e.g. seed data's "/assets/hats/hat_wizard.png") aren't
// reachable via HttpClient, so the handler returns BadRequest in that case
// — those rows need to be edited and re-uploaded through the admin form
// instead. The only supported sources are absolute http(s) URLs.
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
        // Stream the response directly into ImageSharp so the whole PNG
        // doesn't need to land in a single byte[] first — assets are small
        // (sub-MB) but the streaming path avoids the extra copy.
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
            // Image fetched but unscannable (transparent, corrupt, etc.) —
            // clear any stale bounds so the client falls back to slot
            // defaults rather than re-using a previous image's bbox.
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
