using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Models.DTOs;
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
    private readonly ILogger<StreaksController> _logger;

    public StreaksController(IStreakRepository streakRepository, IMapper mapper, ILogger<StreaksController> logger)
    {
        _streakRepository = streakRepository;
        _mapper = mapper;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<StreakDto>>> GetAll([FromQuery] StreakFilterParams filters)
    {
        var userId = GetCurrentUserId();
        filters.UserId = userId;
        _logger.LogDebug("Fetching streaks for user {UserId}", userId);

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
        _logger.LogDebug("Fetching streak {StreakId}", id);
        var streak = await _streakRepository.GetByIdAsync(id);

        if (streak == null || streak.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Streak {StreakId} not found or unauthorized", id);
            return NotFound($"Streak with ID {id} was not found.");
        }

        return Ok(_mapper.Map<StreakDto>(streak));
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<StreakDto>>> GetActive()
    {
        var userId = GetCurrentUserId();
        _logger.LogDebug("Fetching active streaks for user {UserId}", userId);

        var streaks = await _streakRepository.GetActiveByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<StreakDto>>(streaks));
    }

    [HttpPost]
    public async Task<ActionResult<StreakDto>> Create(CreateStreakDto dto)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Creating streak {StreakType} for user {UserId}", dto.StreakType, userId);

        var streak = _mapper.Map<Streak>(dto);
        streak.UserId = userId;
        var created = await _streakRepository.CreateAsync(streak);

        _logger.LogInformation("Streak {StreakId} created for user {UserId}", created.StreakId, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.StreakId }, _mapper.Map<StreakDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateStreakDto dto)
    {
        if (id != dto.StreakId)
            return BadRequest("Streak ID in the URL does not match the request body.");

        _logger.LogInformation("Updating streak {StreakId}", id);
        var streak = await _streakRepository.GetByIdAsync(id);

        if (streak == null || streak.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Streak {StreakId} not found or unauthorized for update", id);
            return NotFound($"Streak with ID {id} was not found.");
        }

        _mapper.Map(dto, streak);
        await _streakRepository.UpdateAsync(streak);

        _logger.LogInformation("Streak {StreakId} updated successfully", id);
        return NoContent();
    }

    [HttpPatch("{id}/increment")]
    public async Task<IActionResult> Increment(int id)
    {
        _logger.LogInformation("Incrementing streak {StreakId}", id);
        var streak = await _streakRepository.GetByIdAsync(id);

        if (streak == null || streak.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Streak {StreakId} not found or unauthorized for increment", id);
            return NotFound($"Streak with ID {id} was not found.");
        }

        await _streakRepository.IncrementAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/reset")]
    public async Task<IActionResult> Reset(int id)
    {
        _logger.LogInformation("Resetting streak {StreakId}", id);
        var streak = await _streakRepository.GetByIdAsync(id);

        if (streak == null || streak.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Streak {StreakId} not found or unauthorized for reset", id);
            return NotFound($"Streak with ID {id} was not found.");
        }

        await _streakRepository.ResetAsync(id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting streak {StreakId}", id);
        var streak = await _streakRepository.GetByIdAsync(id);

        if (streak == null || streak.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Streak {StreakId} not found or unauthorized for deletion", id);
            return NotFound($"Streak with ID {id} was not found.");
        }

        await _streakRepository.DeleteAsync(id);
        _logger.LogInformation("Streak {StreakId} deleted successfully", id);
        return NoContent();
    }
}
