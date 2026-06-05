using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.Users;
using wahaha.API.Models.DTO;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IRequestHandler<GetCurrentUserRequest, UserDto> _me;
    private readonly IRequestHandler<UpdateUserRequest, Unit> _update;
    private readonly IRequestHandler<UploadProfilePictureRequest, UserDto> _uploadPicture;
    private readonly IRequestHandler<DeleteProfilePictureRequest, Unit> _deletePicture;
    private readonly IRequestHandler<AddPointsRequest, Unit> _addPoints;
    private readonly IRequestHandler<SpendPointsRequest, Unit> _spendPoints;
    private readonly IRequestHandler<DeleteAccountRequest, Unit> _deleteAccount;

    public UsersController(
        IRequestHandler<GetCurrentUserRequest, UserDto> me,
        IRequestHandler<UpdateUserRequest, Unit> update,
        IRequestHandler<UploadProfilePictureRequest, UserDto> uploadPicture,
        IRequestHandler<DeleteProfilePictureRequest, Unit> deletePicture,
        IRequestHandler<AddPointsRequest, Unit> addPoints,
        IRequestHandler<SpendPointsRequest, Unit> spendPoints,
        IRequestHandler<DeleteAccountRequest, Unit> deleteAccount)
    {
        _me = me;
        _update = update;
        _uploadPicture = uploadPicture;
        _deletePicture = deletePicture;
        _addPoints = addPoints;
        _spendPoints = spendPoints;
        _deleteAccount = deleteAccount;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
        => (await _me.HandleAsync(new GetCurrentUserRequest(GetCurrentUserId()))).ToActionResult();

    [HttpPut]
    public async Task<IActionResult> Update(UpdateUserDto dto)
        => (await _update.HandleAsync(new UpdateUserRequest(GetCurrentUserId(), dto))).ToActionResult();

    [HttpPost("me/profile-picture")]
    public async Task<ActionResult<UserDto>> UploadProfilePicture(IFormFile file)
        => (await _uploadPicture.HandleAsync(new UploadProfilePictureRequest(GetCurrentUserId(), file))).ToActionResult();

    [HttpDelete("me/profile-picture")]
    public async Task<IActionResult> DeleteProfilePicture()
        => (await _deletePicture.HandleAsync(new DeleteProfilePictureRequest(GetCurrentUserId()))).ToActionResult();

    [HttpPatch("addpoints/{points}")]
    public async Task<IActionResult> AddPoints(int points)
        => (await _addPoints.HandleAsync(new AddPointsRequest(GetCurrentUserId(), points))).ToActionResult();

    [HttpPatch("spendpoints/{points}")]
    public async Task<IActionResult> SpendPoints(int points)
        => (await _spendPoints.HandleAsync(new SpendPointsRequest(GetCurrentUserId(), points))).ToActionResult();

    // Permanently deletes the caller's account and all owned data; client must clear its token after a 204.
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
        => (await _deleteAccount.HandleAsync(new DeleteAccountRequest(GetCurrentUserId()))).ToActionResult();
}
