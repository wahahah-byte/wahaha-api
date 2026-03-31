using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _userRepository.GetAllWithTransactionsAsync();
        return Ok(_mapper.Map<IEnumerable<UserDto>>(users));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await _userRepository.GetByIdWithTransactionsAsync(id);

        if (user == null)
            return NotFound($"User with ID {id} was not found.");

        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<UserDto>> GetByUsername(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);

        if (user == null)
            return NotFound($"User '{username}' was not found.");

        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = _mapper.Map<Users>(dto);
        var created = await _userRepository.CreateAsync(user);

        return CreatedAtAction(nameof(GetById), new { id = created.UserId }, _mapper.Map<UserDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserDto dto)
    {
        if (id != dto.UserId)
            return BadRequest("User ID in the URL does not match the request body.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            return NotFound($"User with ID {id} was not found.");

        _mapper.Map(dto, user);
        await _userRepository.UpdateAsync(user);

        return NoContent();
    }

    [HttpPatch("{id}/addpoints/{points}")]
    public async Task<IActionResult> AddPoints(Guid id, int points)
    {
        if (points <= 0)
            return BadRequest("Points must be a positive number.");

        var success = await _userRepository.AddPointsAsync(id, points);

        if (!success)
            return NotFound($"User with ID {id} was not found.");

        return NoContent();
    }

    [HttpPatch("{id}/spendpoints/{points}")]
    public async Task<IActionResult> SpendPoints(Guid id, int points)
    {
        if (points <= 0)
            return BadRequest("Points must be a positive number.");

        var success = await _userRepository.SpendPointsAsync(id, points);

        if (!success)
            return NotFound($"User with ID {id} was not found or has insufficient balance.");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _userRepository.DeleteAsync(id);

        if (!success)
            return NotFound($"User with ID {id} was not found.");

        return NoContent();
    }
}
