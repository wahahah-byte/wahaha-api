using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.MinigameSessions;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MinigameSessionsController : ControllerBase
{
    private readonly IRequestHandler<GetSessionListRequest, IEnumerable<MinigameSessionDto>> _list;
    private readonly IRequestHandler<GetSessionByIdRequest, MinigameSessionDto> _byId;
    private readonly IRequestHandler<GetSessionsByGameRequest, IEnumerable<MinigameSessionDto>> _byGame;
    private readonly IRequestHandler<GetGameLeaderboardRequest, IEnumerable<MinigameSessionLeaderboardDto>> _leaderboard;
    private readonly IRequestHandler<CreateSessionRequest, MinigameSessionDto> _create;
    private readonly IRequestHandler<DeleteSessionRequest, Unit> _delete;

    public MinigameSessionsController(
        IRequestHandler<GetSessionListRequest, IEnumerable<MinigameSessionDto>> list,
        IRequestHandler<GetSessionByIdRequest, MinigameSessionDto> byId,
        IRequestHandler<GetSessionsByGameRequest, IEnumerable<MinigameSessionDto>> byGame,
        IRequestHandler<GetGameLeaderboardRequest, IEnumerable<MinigameSessionLeaderboardDto>> leaderboard,
        IRequestHandler<CreateSessionRequest, MinigameSessionDto> create,
        IRequestHandler<DeleteSessionRequest, Unit> delete)
    {
        _list = list;
        _byId = byId;
        _byGame = byGame;
        _leaderboard = leaderboard;
        _create = create;
        _delete = delete;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MinigameSessionDto>>> GetAll([FromQuery] MinigameSessionFilterParams filters)
        => (await _list.HandleAsync(new GetSessionListRequest(GetCurrentUserId(), filters))).ToActionResult();

    [HttpGet("{id}")]
    public async Task<ActionResult<MinigameSessionDto>> GetById(int id)
        => (await _byId.HandleAsync(new GetSessionByIdRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<IEnumerable<MinigameSessionDto>>> GetByGame(int gameId)
        => (await _byGame.HandleAsync(new GetSessionsByGameRequest(gameId, GetCurrentUserId()))).ToActionResult();

    [HttpGet("game/{gameId}/leaderboard")]
    public async Task<ActionResult<IEnumerable<MinigameSessionLeaderboardDto>>> GetLeaderboard(int gameId)
        => (await _leaderboard.HandleAsync(new GetGameLeaderboardRequest(gameId))).ToActionResult();

    [HttpPost]
    public async Task<ActionResult<MinigameSessionDto>> Create(CreateMinigameSessionDto dto)
    {
        var result = await _create.HandleAsync(new CreateSessionRequest(GetCurrentUserId(), dto));
        if (result.Status == HandlerStatus.Ok && result.Value != null)
            return CreatedAtAction(nameof(GetById), new { id = result.Value.SessionId }, result.Value);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => (await _delete.HandleAsync(new DeleteSessionRequest(id, GetCurrentUserId()))).ToActionResult();
}
