namespace wahaha.API.Services.Interfaces;

// Tight bounding box of an image's non-transparent pixels in source-pixel
// coordinates (origin at top-left). Returned by IContentBoundsService.
public sealed record ContentBounds(int MinX, int MinY, int MaxX, int MaxY);

public interface IContentBoundsService
{
    // Scans the supplied image for non-transparent pixels and returns the
    // tightest enclosing rectangle, or null when the image has no opaque
    // pixels (or the format is unsupported). The IFormFile stream is read
    // in-full; callers that also need the bytes for upload should call this
    // first and then re-open the stream (IFormFile supports OpenReadStream
    // repeatedly).
    Task<ContentBounds?> ComputeAsync(IFormFile file, CancellationToken ct = default);

    // Stream overload — used by the recompute-bounds endpoint, which fetches
    // an item's PreviewAssetUrl via HttpClient instead of receiving an upload.
    // sourceLabel is just for log messages so failures can be traced back to
    // the URL / blob name that was scanned.
    Task<ContentBounds?> ComputeAsync(Stream stream, string sourceLabel, CancellationToken ct = default);
}
