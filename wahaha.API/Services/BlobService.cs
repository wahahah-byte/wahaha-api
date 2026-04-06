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

    // Lazy initialization — only connects when first used, not at startup
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

    public async Task<string> UploadAsync(IFormFile file, string containerName)
    {
        _logger.LogInformation("Uploading file {FileName} ({Size} bytes) to container {Container}",
            file.FileName, file.Length, containerName);

        var containerClient = GetClient().GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
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

        var containerClient = GetClient().GetBlobContainerClient(containerName);
        var uri = new Uri(fileUrl);
        var blobName = Path.GetFileName(uri.LocalPath);
        var blobClient = containerClient.GetBlobClient(blobName);

        var result = await blobClient.DeleteIfExistsAsync();

        if (result.Value)
            _logger.LogInformation("Blob {BlobName} deleted successfully", blobName);
        else
            _logger.LogWarning("Blob {BlobName} was not found for deletion", blobName);

        return result.Value;
    }
}