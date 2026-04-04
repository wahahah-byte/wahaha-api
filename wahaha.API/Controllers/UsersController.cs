using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
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

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var user = await _userRepository.GetByIdWithTransactionsAsync(GetCurrentUserId());

        if (user == null)
            return NotFound("User was not found.");

        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateUserDto dto)
    {
        var userId = GetCurrentUserId();
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return NotFound("User was not found.");

        dto.UserId = userId;
        _mapper.Map(dto, user);
        await _userRepository.UpdateAsync(user);

        return NoContent();
    }

    [HttpPatch("addpoints/{points}")]
    public async Task<IActionResult> AddPoints(int points)
    {
        if (points <= 0)
            return BadRequest("Points must be a positive number.");

        var success = await _userRepository.AddPointsAsync(GetCurrentUserId(), points);

        if (!success)
            return NotFound("User was not found.");

        return NoContent();
    }

    [HttpPatch("spendpoints/{points}")]
    public async Task<IActionResult> SpendPoints(int points)
    {
        if (points <= 0)
            return BadRequest("Points must be a positive number.");

        var success = await _userRepository.SpendPointsAsync(GetCurrentUserId(), points);

        if (!success)
            return NotFound("User was not found or has insufficient balance.");

        return NoContent();
    }
}
