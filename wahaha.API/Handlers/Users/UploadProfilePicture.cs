using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Handlers.Users;

public sealed record UploadProfilePictureRequest(Guid UserId, IFormFile? File);

public sealed class UploadProfilePictureHandler : IRequestHandler<UploadProfilePictureRequest, UserDto>
{
    private readonly IUserRepository _repo;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;
    private readonly ILogger<UploadProfilePictureHandler> _logger;
    private const long MaxProfilePictureBytes = 1 * 1024 * 1024;
    private const string ProfilePictureContainer = "profile-pictures";

    public UploadProfilePictureHandler(
        IUserRepository repo,
        IBlobService blobService,
        IMapper mapper,
        ILogger<UploadProfilePictureHandler> logger)
    {
        _repo = repo;
        _blobService = blobService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<UserDto>> HandleAsync(UploadProfilePictureRequest request, CancellationToken ct = default)
    {
        var file = request.File;
        if (file == null || file.Length == 0)
            return HandlerResult<UserDto>.BadRequest("File is required.");
        if (file.Length > MaxProfilePictureBytes)
            return HandlerResult<UserDto>.BadRequest($"File is too large. Max {MaxProfilePictureBytes / 1024} KB.");
        if (string.IsNullOrEmpty(file.ContentType) || !file.ContentType.StartsWith("image/"))
            return HandlerResult<UserDto>.BadRequest("File must be an image.");

        var user = await _repo.GetByIdAsync(request.UserId);
        if (user == null) return HandlerResult<UserDto>.NotFound("User was not found.");

        // Upload first then delete old blob: leaking on failure beats losing the picture.
        var previousUrl = user.ProfilePictureUrl;

        var url = await _blobService.UploadAsync(file, ProfilePictureContainer);
        user.ProfilePictureUrl = url;
        await _repo.UpdateAsync(user);

        if (!string.IsNullOrEmpty(previousUrl))
        {
            try { await _blobService.DeleteAsync(previousUrl, ProfilePictureContainer); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete previous profile picture {Url}", previousUrl); }
        }

        return HandlerResult<UserDto>.Ok(_mapper.Map<UserDto>(user));
    }
}
