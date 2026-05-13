using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Services;

// Computes the tight bounding box of non-transparent pixels in an uploaded
// image. Used by the avatar-item create / update handlers so the frontend
// can centre the visible content of a PNG inside its inventory card without
// hand-tuned per-item transform overrides.
//
// Thresholding: pixels with alpha greater than ALPHA_THRESHOLD count as
// content. The default of 16 (out of 255) is lenient — it includes faint
// antialiasing fringes so the bbox doesn't crop into soft edges. Bumping
// this higher produces tighter crops but can shave off legitimate pixels.
public class ContentBoundsService : IContentBoundsService
{
    // 16/255 ≈ 6%. Anything below this is treated as transparent for bbox
    // purposes. Calibrated empirically against the existing avatar PNGs.
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

            // Row-major scan. ImageSharp's ProcessPixelRows hands back a
            // span per row which lets us scan in tight bounds without the
            // per-pixel allocations of GetPixel/SetPixel.
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

            // Fully transparent / unreadable image — no content to centre.
            // Caller stores null bounds and the client falls back to the
            // slot defaults.
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
            // Don't fail the upload — bbox is an enhancement; missing it
            // just means we fall back to slot defaults. Log enough info to
            // diagnose later (corrupt PNGs, unsupported formats, etc.).
            _logger.LogWarning(ex,
                "ContentBoundsService: failed to compute bounds for {Source}",
                sourceLabel);
            return null;
        }
    }
}
