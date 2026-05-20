using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Services;

// Computes tight bbox of non-transparent pixels (alpha > AlphaThreshold).
public class ContentBoundsService : IContentBoundsService
{
    // 16/255 ≈ 6% alpha cutoff; lenient to preserve AA fringes.
    private const int AlphaThreshold = 16;

    private readonly ILogger<ContentBoundsService> _logger;

    public ContentBoundsService(ILogger<ContentBoundsService> logger)
    {
        _logger = logger;
    }

    public async Task<ContentBounds?> ComputeAsync(IFormFile file, CancellationToken ct = default)
    {
        await using var stream = file.OpenReadStream();
        return await ComputeAsync(stream, file.FileName, ct);
    }

    public async Task<ContentBounds?> ComputeAsync(Stream stream, string sourceLabel, CancellationToken ct = default)
    {
        try
        {
            using var image = await Image.LoadAsync<Rgba32>(stream, ct);

            int minX = image.Width, minY = image.Height, maxX = -1, maxY = -1;

            // Row-major span scan via ProcessPixelRows (no per-pixel allocations).
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        if (row[x].A > AlphaThreshold)
                        {
                            if (x < minX) minX = x;
                            if (y < minY) minY = y;
                            if (x > maxX) maxX = x;
                            if (y > maxY) maxY = y;
                        }
                    }
                }
            });

            // No opaque pixels found; caller stores null bounds (slot defaults).
            if (maxX < 0 || maxY < 0)
            {
                _logger.LogWarning(
                    "ContentBoundsService: no opaque pixels in {Source} (alpha > {Threshold})",
                    sourceLabel, AlphaThreshold);
                return null;
            }

            _logger.LogInformation(
                "ContentBoundsService: {Source} → bbox=({MinX},{MinY})..({MaxX},{MaxY}) in {W}×{H}",
                sourceLabel, minX, minY, maxX, maxY, image.Width, image.Height);

            return new ContentBounds(minX, minY, maxX, maxY);
        }
        catch (Exception ex)
        {
            // Bbox failure shouldn't fail the upload; null falls back to slot defaults.
            _logger.LogWarning(ex,
                "ContentBoundsService: failed to compute bounds for {Source}",
                sourceLabel);
            return null;
        }
    }
}
