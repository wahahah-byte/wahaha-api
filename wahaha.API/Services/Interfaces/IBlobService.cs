namespace wahaha.API.Services.Interfaces;

public interface IBlobService
{
    Task<string> UploadAsync(IFormFile file, string containerName);
    Task<bool> DeleteAsync(string fileUrl, string containerName);
}
