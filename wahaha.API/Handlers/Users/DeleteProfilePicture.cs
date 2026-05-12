using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Handlers.Users;

public sealed record DeleteProfilePictureRequest(Guid UserId);

public sealed class DeleteProfilePictureHandler : IRequestHandler<DeleteProfilePictureRequest, Unit>
{
    private readonly IUserRepository _repo;
    private readonly IBlobService _blobService;
    private readonly ILogger<DeleteProfilePictureHandler> _logger;
    private const string ProfilePictureContainer = "profile-pictures";

    public DeleteProfilePictureHandler(IUserRepository repo, IBlobService blobService, ILogger<DeleteProfilePictureHandler> logger)
    {
        _repo = repo;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(DeleteProfilePictureRequest request, CancellationToken ct = default)
    {
        var user = await _repo.GetByIdAsync(request.UserId);
        if (user == null) return HandlerResult<Unit>.NotFound("User was not found.");

        var url = user.ProfilePictureUrl;
        if (string.IsNullOrEmpty(url)) return HandlerResult<Unit>.NoContent();

        user.ProfilePictureUrl = null;
        await _repo.UpdateAsync(user);

        try { await _blobService.DeleteAsync(url, ProfilePictureContainer); }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete profile picture blob {Url}", url); }

        return HandlerResult<Unit>.NoContent();
    }
}
