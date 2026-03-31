using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MinigameSessionDto>>> GetAll()
    {
        var sessions = await _sessionRepository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<MinigameSessionDto>>(sessions));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MinigameSessionDto>> GetById(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);

        if (session == null)
            return NotFound($"Session with ID {id} was not found.");

        return Ok(_mapper.Map<MinigameSessionDto>(session));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<MinigameSessionDto>>> GetByUser(Guid userId)
    {
        var sessions = await _sessionRepository.GetByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<MinigameSessionDto>>(sessions));
    }

    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<IEnumerable<MinigameSessionDto>>> GetByGame(int gameId)
    {
        var sessions = await _sessionRepository.GetByGameAsync(gameId);
        return Ok(_mapper.Map<IEnumerable<MinigameSessionDto>>(sessions));
    }

    [HttpGet("game/{gameId}/leaderboard")]
    public async Task<ActionResult<IEnumerable<MinigameSessionLeaderboardDto>>> GetLeaderboard(int gameId)
    {
        var leaderboard = await _sessionRepository.GetLeaderboardAsync(gameId);
        return Ok(_mapper.Map<IEnumerable<MinigameSessionLeaderboardDto>>(leaderboard));
    }

    [HttpPost]
    public async Task<ActionResult<MinigameSessionDto>> Create(CreateMinigameSessionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var session = _mapper.Map<MinigameSession>(dto);
        var created = await _sessionRepository.CreateAsync(session);

        return CreatedAtAction(nameof(GetById), new { id = created.SessionId }, _mapper.Map<MinigameSessionDto>(created));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _sessionRepository.DeleteAsync(id);

        if (!success)
            return NotFound($"Session with ID {id} was not found.");

        return NoContent();
    }
}
