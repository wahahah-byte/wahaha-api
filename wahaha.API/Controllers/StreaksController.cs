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
public class StreaksController : ControllerBase
{
    private readonly IStreakRepository _streakRepository;
    private readonly IMapper _mapper;

    public StreaksController(IStreakRepository streakRepository, IMapper mapper)
    {
        _streakRepository = streakRepository;
        _mapper = mapper;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<StreakDto>>> GetAll([FromQuery] StreakFilterParams filters)
    {
        filters.UserId = GetCurrentUserId();
        var result = await _streakRepository.GetFilteredAsync(filters);

        return Ok(new PagedResult<StreakDto>
        {
            Data = _mapper.Map<IEnumerable<StreakDto>>(result.Data),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StreakDto>> GetById(int id)
    {
        var streak = await _streakRepository.GetByIdAsync(id);

        if (streak == null || streak.UserId != GetCurrentUserId())
            return NotFound($"Streak with ID {id} was not found.");

        return Ok(_mapper.Map<StreakDto>(streak));
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<StreakDto>>> GetActive()
    {
        var streaks = await _streakRepository.GetActiveByUserAsync(GetCurrentUserId());
        return Ok(_mapper.Map<IEnumerable<StreakDto>>(streaks));
    }

    [HttpPost]
    public async Task<ActionResult<StreakDto>> Create(CreateStreakDto dto)
    {
        var streak = _mapper.Map<Streak>(dto);
        streak.UserId = GetCurrentUserId();
        var created = await _streakRepository.CreateAsync(streak);

        return CreatedAtAction(nameof(GetById), new { id = created.StreakId }, _mapper.Map<StreakDto>(created));
    }

    [HttpPatch("{id}/increment")]
    public async Task<IActionResult> Increment(int id)
    {
        var streak = await _streakRepository.GetByIdAsync(id);

        if (streak == null || streak.UserId != GetCurrentUserId())
            return NotFound($"Streak with ID {id} was not found.");

        await _streakRepository.IncrementAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/reset")]
    public async Task<IActionResult> Reset(int id)
    {
        var streak = await _streakRepository.GetByIdAsync(id);

        if (streak == null || streak.UserId != GetCurrentUserId())
            return NotFound($"Streak with ID {id} was not found.");

        await _streakRepository.ResetAsync(id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var streak = await _streakRepository.GetByIdAsync(id);

        if (streak == null || streak.UserId != GetCurrentUserId())
            return NotFound($"Streak with ID {id} was not found.");

        await _streakRepository.DeleteAsync(id);
        return NoContent();
    }
}
