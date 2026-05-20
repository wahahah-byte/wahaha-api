using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Services;

public class BlobService : IBlobService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BlobService> _logger;
    private BlobServiceClient? _blobServiceClient;

    public BlobService(IConfiguration configuration, ILogger<BlobService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // Lazy init: connect on first use, not at startup.
    private BlobServiceClient GetClient()
    {
        if (_blobServiceClient == null)
        {
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");

            if (string.IsNullOrEmpty(connectionString) || connectionString == "set-in-user-secrets")
                throw new InvalidOperationException(
                    "AzureBlobStorage connection string is not configured. Add it to User Secrets.");

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        return _blobServiceClient;
    }

    public async Task<string> UploadAsync(IFormFile file, string containerName, string? desiredName = null)
    {
        _logger.LogInformation("Uploading file {FileName} ({Size} bytes) to container {Container}",
            file.FileName, file.Length, containerName);

        var containerClient = GetClient().GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension)) extension = ".png";

        // Named uploads slugify + collision-resolve; nameless falls back to GUID.
        string fileName;
        if (string.IsNullOrWhiteSpace(desiredName))
        {
            fileName = $"{Guid.NewGuid()}{extension}";
        }
        else
        {
            var slug = Slugify(desiredName);
            fileName = await ResolveUniqueNameAsync(containerClient, slug, extension);
        }
        var blobClient = containerClient.GetBlobClient(fileName);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = file.ContentType
        };

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders
        });

        var url = blobClient.Uri.ToString();
        _logger.LogInformation("File uploaded successfully to {Url}", url);

        return url;
    }

    public async Task<bool> DeleteAsync(string fileUrl, string containerName)
    {
        _logger.LogInformation("Deleting blob {FileUrl} from container {Container}", fileUrl, containerName);

        // Relative / third-party URLs aren't ours to delete; skip the cleanup quietly.
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            _logger.LogWarning("Skipping blob delete — {FileUrl} is not an absolute URI", fileUrl);
            return false;
        }

        var containerClient = GetClient().GetBlobContainerClient(containerName);
        var blobName = Path.GetFileName(uri.LocalPath);
        var blobClient = containerClient.GetBlobClient(blobName);

        var result = await blobClient.DeleteIfExistsAsync();

        if (result.Value)
            _logger.LogInformation("Blob {BlobName} deleted successfully", blobName);
        else
            _logger.LogWarning("Blob {BlobName} was not found for deletion", blobName);

        return result.Value;
    }

    // Slugify to lowercase [a-z0-9_-]; non-matches collapse to "_"; empty falls back to "item".
    private static string Slugify(string source)
    {
        var lower = source.Trim().ToLowerInvariant();
        var sb = new StringBuilder(lower.Length);
        var lastWasSep = false;
        foreach (var c in lower)
        {
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_')
            {
                sb.Append(c);
                lastWasSep = false;
            }
            else if (!lastWasSep)
            {
                sb.Append('_');
                lastWasSep = true;
            }
        }
        var result = sb.ToString().Trim('_', '-');
        return string.IsNullOrEmpty(result) ? "item" : result;
    }

    // Pick a non-colliding blob name: bare slug, then "{slug}-2..100", then GUID suffix.
    private static async Task<string> ResolveUniqueNameAsync(BlobContainerClient container, string baseSlug, string extension)
    {
        var candidate = $"{baseSlug}{extension}";
        if (!(await container.GetBlobClient(candidate).ExistsAsync()).Value)
            return candidate;
        for (int n = 2; n <= 100; n++)
        {
            candidate = $"{baseSlug}-{n}{extension}";
            if (!(await container.GetBlobClient(candidate).ExistsAsync()).Value)
                return candidate;
        }
        return $"{baseSlug}-{Guid.NewGuid():N}{extension}";
    }
}