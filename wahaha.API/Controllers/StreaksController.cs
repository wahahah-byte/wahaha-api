using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreaksController : ControllerBase
{
    private readonly IStreakRepository _streakRepository;
    private readonly IMapper _mapper;

    public StreaksController(IStreakRepository streakRepository, IMapper mapper)
    {
        _streakRepository = streakRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StreakDto>>> GetAll()
    {
        var streaks = await _streakRepository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<StreakDto>>(streaks));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StreakDto>> GetById(int id)
    {
        var streak = await _streakRepository.GetByIdAsync(id);

        if (streak == null)
            return NotFound($"Streak with ID {id} was not found.");

        return Ok(_mapper.Map<StreakDto>(streak));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<StreakDto>>> GetByUser(Guid userId)
    {
        var streaks = await _streakRepository.GetByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<StreakDto>>(streaks));
    }

    [HttpGet("user/{userId}/active")]
    public async Task<ActionResult<IEnumerable<StreakDto>>> GetActiveByUser(Guid userId)
    {
        var streaks = await _streakRepository.GetActiveByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<StreakDto>>(streaks));
    }

    [HttpPost]
    public async Task<ActionResult<StreakDto>> Create(CreateStreakDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var streak = _mapper.Map<Streak>(dto);
        var created = await _streakRepository.CreateAsync(streak);

        return CreatedAtAction(nameof(GetById), new { id = created.StreakId }, _mapper.Map<StreakDto>(created));
    }

    [HttpPatch("{id}/increment")]
    public async Task<IActionResult> Increment(int id)
    {
        var success = await _streakRepository.IncrementAsync(id);

        if (!success)
            return NotFound($"Streak with ID {id} was not found.");

        return NoContent();
    }

    [HttpPatch("{id}/reset")]
    public async Task<IActionResult> Reset(int id)
    {
        var success = await _streakRepository.ResetAsync(id);

        if (!success)
            return NotFound($"Streak with ID {id} was not found.");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _streakRepository.DeleteAsync(id);

        if (!success)
            return NotFound($"Streak with ID {id} was not found.");

        return NoContent();
    }
}
