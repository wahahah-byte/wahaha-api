using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MinigameSessionsController : ControllerBase
{
    private readonly IMinigameSessionRepository _sessionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<MinigameSessionsController> _logger;

    public MinigameSessionsController(
        IMinigameSessionRepository sessionRepository,
        IMapper mapper,
        ILogger<MinigameSessionsController> logger)
    {
        _sessionRepository = sessionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MinigameSessionDto>>> GetAll([FromQuery] MinigameSessionFilterParams filters)
    {
        var userId = GetCurrentUserId();
        filters.UserId = userId;
        _logger.LogDebug("Fetching sessions for user {UserId}", userId);

        var sessions = await _sessionRepository.GetFilteredAsync(filters);
        return Ok(_mapper.Map<IEnumerable<MinigameSessionDto>>(sessions));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MinigameSessionDto>> GetById(int id)
    {
        _logger.LogDebug("Fetching session {SessionId}", id);
        var session = await _sessionRepository.GetByIdAsync(id);

        if (session == null || session.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Session {SessionId} not found or unauthorized", id);
            return NotFound($"Session with ID {id} was not found.");
        }

        return Ok(_mapper.Map<MinigameSessionDto>(session));
    }

    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<IEnumerable<MinigameSessionDto>>> GetByGame(int gameId)
    {
        var userId = GetCurrentUserId();
        _logger.LogDebug("Fetching sessions for game {GameId} by user {UserId}", gameId, userId);

        var filters = new MinigameSessionFilterParams
        {
            UserId = userId,
            GameId = gameId
        };
        var sessions = await _sessionRepository.GetFilteredAsync(filters);
        return Ok(_mapper.Map<IEnumerable<MinigameSessionDto>>(sessions));
    }

    [HttpGet("game/{gameId}/leaderboard")]
    public async Task<ActionResult<IEnumerable<MinigameSessionLeaderboardDto>>> GetLeaderboard(int gameId)
    {
        _logger.LogDebug("Fetching leaderboard for game {GameId}", gameId);
        var leaderboard = await _sessionRepository.GetLeaderboardAsync(gameId);
        return Ok(_mapper.Map<IEnumerable<MinigameSessionLeaderboardDto>>(leaderboard));
    }

    [HttpPost]
    public async Task<ActionResult<MinigameSessionDto>> Create(CreateMinigameSessionDto dto)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Creating session for game {GameId} by user {UserId}", dto.GameId, userId);

        var session = _mapper.Map<MinigameSession>(dto);
        session.UserId = userId;
        var created = await _sessionRepository.CreateAsync(session);

        _logger.LogInformation("Session {SessionId} created for game {GameId} by user {UserId}",
            created.SessionId, dto.GameId, userId);

        return CreatedAtAction(nameof(GetById), new { id = created.SessionId },
            _mapper.Map<MinigameSessionDto>(created));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting session {SessionId}", id);
        var session = await _sessionRepository.GetByIdAsync(id);

        if (session == null || session.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Session {SessionId} not found or unauthorized for deletion", id);
            return NotFound($"Session with ID {id} was not found.");
        }

        await _sessionRepository.DeleteAsync(id);
        _logger.LogInformation("Session {SessionId} deleted successfully", id);
        return NoContent();
    }
}
