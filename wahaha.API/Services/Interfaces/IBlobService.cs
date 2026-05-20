namespace wahaha.API.Services.Interfaces;

public interface IBlobService
{
    // desiredName: optional base name (slugified + collision-resolved); null = GUID naming.
    Task<string> UploadAsync(IFormFile file, string containerName, string? desiredName = null);
    Task<bool> DeleteAsync(string fileUrl, string containerName);
}
