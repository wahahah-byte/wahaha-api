using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;
    // Profile pictures are pre-resized client-side to ~256x256 before upload,
    // so anything above 1 MB is almost certainly the raw camera roll image
    // and should be rejected. Bumping this also requires raising
    // FormOptions.MultipartBodyLengthLimit in Program.cs.
    private const long MaxProfilePictureBytes = 1 * 1024 * 1024;
    private const string ProfilePictureContainer = "profile-pictures";

    public UsersController(
        IUserRepository userRepository,
        IBlobService blobService,
        IMapper mapper,
        ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _blobService = blobService;
        _mapper = mapper;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        _logger.LogDebug("Fetching profile for user {UserId}", userId);

        var user = await _userRepository.GetByIdWithTransactionsAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return NotFound("User was not found.");
        }

        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateUserDto dto)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Updating profile for user {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for update", userId);
            return NotFound("User was not found.");
        }

        dto.UserId = userId;
        _mapper.Map(dto, user);
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Profile updated for user {UserId}", userId);
        return NoContent();
    }

    // Upload (or replace) the current user's profile picture. Expects a
    // multipart/form-data body with a single `file` part. The frontend
    // resizes to ~256x256 before sending, so we cap the body size aggressively
    // and validate that the file is actually an image.
    [HttpPost("me/profile-picture")]
    public async Task<ActionResult<UserDto>> UploadProfilePicture(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required.");
        if (file.Length > MaxProfilePictureBytes)
            return BadRequest($"File is too large. Max {MaxProfilePictureBytes / 1024} KB.");
        if (string.IsNullOrEmpty(file.ContentType) || !file.ContentType.StartsWith("image/"))
            return BadRequest("File must be an image.");

        var userId = GetCurrentUserId();
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound("User was not found.");

        // If we already have a picture, delete it after we have the new URL —
        // ordering matters: better to leak an old blob on failure than to
        // delete first and end up with no picture if the upload fails.
        var previousUrl = user.ProfilePictureUrl;

        var url = await _blobService.UploadAsync(file, ProfilePictureContainer);
        user.ProfilePictureUrl = url;
        await _userRepository.UpdateAsync(user);

        if (!string.IsNullOrEmpty(previousUrl))
        {
            try { await _blobService.DeleteAsync(previousUrl, ProfilePictureContainer); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete previous profile picture {Url}", previousUrl); }
        }

        return Ok(_mapper.Map<UserDto>(user));
    }

    // Remove the current user's profile picture. Best-effort blob delete:
    // we always clear the column even if blob delete fails (the URL would
    // 404 anyway and the client falls back to the default avatar).
    [HttpDelete("me/profile-picture")]
    public async Task<IActionResult> DeleteProfilePicture()
    {
        var userId = GetCurrentUserId();
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound("User was not found.");

        var url = user.ProfilePictureUrl;
        if (string.IsNullOrEmpty(url))
            return NoContent();

        user.ProfilePictureUrl = null;
        await _userRepository.UpdateAsync(user);

        try { await _blobService.DeleteAsync(url, ProfilePictureContainer); }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete profile picture blob {Url}", url); }

        return NoContent();
    }

    [HttpPatch("addpoints/{points}")]
    public async Task<IActionResult> AddPoints(int points)
    {
        if (points <= 0)
            return BadRequest("Points must be a positive number.");

        var userId = GetCurrentUserId();
        _logger.LogInformation("Adding {Points} points to user {UserId}", points, userId);

        var success = await _userRepository.AddPointsAsync(userId, points);

        if (!success)
        {
            _logger.LogWarning("User {UserId} not found when adding points", userId);
            return NotFound("User was not found.");
        }

        return NoContent();
    }

    [HttpPatch("spendpoints/{points}")]
    public async Task<IActionResult> SpendPoints(int points)
    {
        if (points <= 0)
            return BadRequest("Points must be a positive number.");

        var userId = GetCurrentUserId();
        _logger.LogInformation("Spending {Points} points for user {UserId}", points, userId);

        var success = await _userRepository.SpendPointsAsync(userId, points);

        if (!success)
        {
            _logger.LogWarning("User {UserId} not found or insufficient balance when spending {Points} points",
                userId, points);
            return NotFound("User was not found or has insufficient balance.");
        }

        return NoContent();
    }
}
