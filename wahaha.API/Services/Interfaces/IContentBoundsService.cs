namespace wahaha.API.Services.Interfaces;

// Tight bbox of non-transparent pixels in source-pixel coords (top-left origin).
public sealed record ContentBounds(int MinX, int MinY, int MaxX, int MaxY);

public interface IContentBoundsService
{
    // Scan an upload for non-transparent pixels; null if none / unsupported format.
    Task<ContentBounds?> ComputeAsync(IFormFile file, CancellationToken ct = default);

    // Stream overload used by the recompute-bounds endpoint; sourceLabel for logging.
    Task<ContentBounds?> ComputeAsync(Stream stream, string sourceLabel, CancellationToken ct = default);
}
