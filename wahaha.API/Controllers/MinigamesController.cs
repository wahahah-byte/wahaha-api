using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Auth;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MinigamesController : ControllerBase
{
    private readonly IMinigameRepository _minigameRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<MinigamesController> _logger;

    public MinigamesController(
        IMinigameRepository minigameRepository,
        IMapper mapper,
        ILogger<MinigamesController> logger)
    {
        _minigameRepository = minigameRepository;
        _mapper = mapper;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MinigameDto>>> GetAll([FromQuery] MinigameFilterParams filters)
    {
        _logger.LogDebug("Fetching minigames with filters");
        var games = await _minigameRepository.GetFilteredAsync(filters);
        return Ok(_mapper.Map<IEnumerable<MinigameDto>>(games));
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<MinigameDto>> GetById(int id)
    {
        _logger.LogDebug("Fetching minigame {GameId}", id);
        var game = await _minigameRepository.GetByIdAsync(id);

        if (game == null)
        {
            _logger.LogWarning("Minigame {GameId} not found", id);
            return NotFound($"Minigame with ID {id} was not found.");
        }

        return Ok(_mapper.Map<MinigameDto>(game));
    }

    [AllowAnonymous]
    [HttpGet("unlocked/{userLevel}")]
    public async Task<ActionResult<IEnumerable<MinigameDto>>> GetUnlocked(int userLevel)
    {
        _logger.LogDebug("Fetching minigames unlocked for level {Level}", userLevel);
        var games = await _minigameRepository.GetUnlockedAsync(userLevel);
        return Ok(_mapper.Map<IEnumerable<MinigameDto>>(games));
    }

    [AllowAnonymous]
    [HttpGet("difficulty/{difficulty}")]
    public async Task<ActionResult<IEnumerable<MinigameDto>>> GetByDifficulty(Difficulty difficulty)
    {
        _logger.LogDebug("Fetching minigames by difficulty {Difficulty}", difficulty);
        var games = await _minigameRepository.GetByDifficultyAsync(difficulty);
        return Ok(_mapper.Map<IEnumerable<MinigameDto>>(games));
    }

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPost]
    public async Task<ActionResult<MinigameDto>> Create(CreateMinigameDto dto)
    {
        _logger.LogInformation("Creating minigame {Name}", dto.Name);
        var game = _mapper.Map<Minigame>(dto);
        var created = await _minigameRepository.CreateAsync(game);

        _logger.LogInformation("Minigame {GameId} ({Name}) created successfully", created.GameId, created.Name);
        return CreatedAtAction(nameof(GetById), new { id = created.GameId }, _mapper.Map<MinigameDto>(created));
    }

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateMinigameDto dto)
    {
        if (id != dto.GameId)
            return BadRequest("Game ID in the URL does not match the request body.");

        _logger.LogInformation("Updating minigame {GameId}", id);
        var game = await _minigameRepository.GetByIdAsync(id);

        if (game == null)
        {
            _logger.LogWarning("Minigame {GameId} not found for update", id);
            return NotFound($"Minigame with ID {id} was not found.");
        }

        _mapper.Map(dto, game);
        await _minigameRepository.UpdateAsync(game);

        _logger.LogInformation("Minigame {GameId} updated successfully", id);
        return NoContent();
    }

    [Authorize(Roles = WahahaUserRoles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting minigame {GameId}", id);
        var success = await _minigameRepository.DeleteAsync(id);

        if (!success)
        {
            _logger.LogWarning("Minigame {GameId} not found for deletion", id);
            return NotFound($"Minigame with ID {id} was not found.");
        }

        _logger.LogInformation("Minigame {GameId} deleted successfully", id);
        return NoContent();
    }
}
