using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.Minigames;
using wahaha.API.Models.Auth;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;

namespace wahaha.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MinigamesController : ControllerBase
{
    private readonly IRequestHandler<GetMinigameListRequest, IEnumerable<MinigameDto>> _list;
    private readonly IRequestHandler<GetMinigameByIdRequest, MinigameDto> _byId;
    private readonly IRequestHandler<GetUnlockedMinigamesRequest, IEnumerable<MinigameDto>> _unlocked;
    private readonly IRequestHandler<GetMinigamesByDifficultyRequest, IEnumerable<MinigameDto>> _byDifficulty;
    private readonly IRequestHandler<CreateMinigameRequest, MinigameDto> _create;
    private readonly IRequestHandler<UpdateMinigameRequest, Unit> _update;
    private readonly IRequestHandler<DeleteMinigameRequest, Unit> _delete;

    public MinigamesController(
        IRequestHandler<GetMinigameListRequest, IEnumerable<MinigameDto>> list,
        IRequestHandler<GetMinigameByIdRequest, MinigameDto> byId,
        IRequestHandler<GetUnlockedMinigamesRequest, IEnumerable<MinigameDto>> unlocked,
        IRequestHandler<GetMinigamesByDifficultyRequest, IEnumerable<MinigameDto>> byDifficulty,
        IRequestHandler<CreateMinigameRequest, MinigameDto> create,
        IRequestHandler<UpdateMinigameRequest, Unit> update,
        IRequestHandler<DeleteMinigameRequest, Unit> delete)
    {
        _list = list;
        _byId = byId;
        _unlocked = unlocked;
        _byDifficulty = byDifficulty;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MinigameDto>>> GetAll([FromQuery] MinigameFilterParams filters)
        => (await _list.HandleAsync(new GetMinigameListRequest(filters))).ToActionResult();

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<MinigameDto>> GetById(int id)
        => (await _byId.HandleAsync(new GetMinigameByIdRequest(id))).ToActionResult();

    [AllowAnonymous]
    [HttpGet("unlocked/{userLevel}")]
    public async Task<ActionResult<IEnumerable<MinigameDto>>> GetUnlocked(int userLevel)
        => (await _unlocked.HandleAsync(new GetUnlockedMinigamesRequest(userLevel))).ToActionResult();

    [AllowAnonymous]
    [HttpGet("difficulty/{difficulty}")]
    public async Task<ActionResult<IEnumerable<MinigameDto>>> GetByDifficulty(Difficulty difficulty)
        => (await _byDifficulty.HandleAsync(new GetMinigamesByDifficultyRequest(difficulty))).ToActionResult();

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPost]
    public async Task<ActionResult<MinigameDto>> Create(CreateMinigameDto dto)
    {
        var result = await _create.HandleAsync(new CreateMinigameRequest(dto));
        if (result.Status == HandlerStatus.Ok && result.Value != null)
            return CreatedAtAction(nameof(GetById), new { id = result.Value.GameId }, result.Value);
        return result.ToActionResult();
    }

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateMinigameDto dto)
        => (await _update.HandleAsync(new UpdateMinigameRequest(id, dto))).ToActionResult();

    [Authorize(Roles = WahahaUserRoles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => (await _delete.HandleAsync(new DeleteMinigameRequest(id))).ToActionResult();
}
