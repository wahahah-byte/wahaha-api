using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository userRepository, IMapper mapper, ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
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
