using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
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

    public MinigameSessionsController(IMinigameSessionRepository sessionRepository, IMapper mapper)
    {
        _sessionRepository = sessionRepository;
        _mapper = mapper;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MinigameSessionDto>>> GetAll([FromQuery] MinigameSessionFilterParams filters)
    {
        filters.UserId = GetCurrentUserId();
        var sessions = await _sessionRepository.GetFilteredAsync(filters);
        return Ok(_mapper.Map<IEnumerable<MinigameSessionDto>>(sessions));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MinigameSessionDto>> GetById(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);

        if (session == null || session.UserId != GetCurrentUserId())
            return NotFound($"Session with ID {id} was not found.");

        return Ok(_mapper.Map<MinigameSessionDto>(session));
    }

    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<IEnumerable<MinigameSessionDto>>> GetByGame(int gameId)
    {
        var filters = new MinigameSessionFilterParams
        {
            UserId = GetCurrentUserId(),
            GameId = gameId
        };
        var sessions = await _sessionRepository.GetFilteredAsync(filters);
        return Ok(_mapper.Map<IEnumerable<MinigameSessionDto>>(sessions));
    }

    [HttpGet("game/{gameId}/leaderboard")]
    public async Task<ActionResult<IEnumerable<MinigameSessionLeaderboardDto>>> GetLeaderboard(int gameId)
    {
        // Leaderboard is public — shows all users for a game
        var leaderboard = await _sessionRepository.GetLeaderboardAsync(gameId);
        return Ok(_mapper.Map<IEnumerable<MinigameSessionLeaderboardDto>>(leaderboard));
    }

    [HttpPost]
    public async Task<ActionResult<MinigameSessionDto>> Create(CreateMinigameSessionDto dto)
    {
        var session = _mapper.Map<MinigameSession>(dto);
        session.UserId = GetCurrentUserId();
        var created = await _sessionRepository.CreateAsync(session);

        return CreatedAtAction(nameof(GetById), new { id = created.SessionId }, _mapper.Map<MinigameSessionDto>(created));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);

        if (session == null || session.UserId != GetCurrentUserId())
            return NotFound($"Session with ID {id} was not found.");

        await _sessionRepository.DeleteAsync(id);
        return NoContent();
    }
}
