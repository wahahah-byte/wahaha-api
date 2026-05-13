namespace wahaha.API.Services.Interfaces;

public interface IBlobService
{
    // desiredName: human-readable base name (without extension) the caller
    // wants the blob saved under. The implementation slugifies it
    // (lowercase, [a-z0-9_-] only) and adds a "-2", "-3", … suffix if a
    // blob with that name already exists. When null/empty the legacy
    // GUID naming is preserved so callers that don't care (e.g. profile
    // pictures) keep working unchanged.
    Task<string> UploadAsync(IFormFile file, string containerName, string? desiredName = null);
    Task<bool> DeleteAsync(string fileUrl, string containerName);
}
