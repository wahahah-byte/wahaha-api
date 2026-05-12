using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.Streaks;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StreaksController : ControllerBase
{
    private readonly IRequestHandler<GetStreakListRequest, PagedResult<StreakDto>> _list;
    private readonly IRequestHandler<GetStreakByIdRequest, StreakDto> _byId;
    private readonly IRequestHandler<GetActiveStreaksRequest, IEnumerable<StreakDto>> _active;
    private readonly IRequestHandler<CreateStreakRequest, StreakDto> _create;
    private readonly IRequestHandler<UpdateStreakRequest, Unit> _update;
    private readonly IRequestHandler<IncrementStreakRequest, Unit> _increment;
    private readonly IRequestHandler<ResetStreakRequest, Unit> _reset;
    private readonly IRequestHandler<DeleteStreakRequest, Unit> _delete;

    public StreaksController(
        IRequestHandler<GetStreakListRequest, PagedResult<StreakDto>> list,
        IRequestHandler<GetStreakByIdRequest, StreakDto> byId,
        IRequestHandler<GetActiveStreaksRequest, IEnumerable<StreakDto>> active,
        IRequestHandler<CreateStreakRequest, StreakDto> create,
        IRequestHandler<UpdateStreakRequest, Unit> update,
        IRequestHandler<IncrementStreakRequest, Unit> increment,
        IRequestHandler<ResetStreakRequest, Unit> reset,
        IRequestHandler<DeleteStreakRequest, Unit> delete)
    {
        _list = list;
        _byId = byId;
        _active = active;
        _create = create;
        _update = update;
        _increment = increment;
        _reset = reset;
        _delete = delete;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<StreakDto>>> GetAll([FromQuery] StreakFilterParams filters)
        => (await _list.HandleAsync(new GetStreakListRequest(GetCurrentUserId(), filters))).ToActionResult();

    [HttpGet("{id}")]
    public async Task<ActionResult<StreakDto>> GetById(int id)
        => (await _byId.HandleAsync(new GetStreakByIdRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<StreakDto>>> GetActive()
        => (await _active.HandleAsync(new GetActiveStreaksRequest(GetCurrentUserId()))).ToActionResult();

    [HttpPost]
    public async Task<ActionResult<StreakDto>> Create(CreateStreakDto dto)
    {
        var result = await _create.HandleAsync(new CreateStreakRequest(GetCurrentUserId(), dto));
        if (result.Status == HandlerStatus.Ok && result.Value != null)
            return CreatedAtAction(nameof(GetById), new { id = result.Value.StreakId }, result.Value);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateStreakDto dto)
        => (await _update.HandleAsync(new UpdateStreakRequest(id, GetCurrentUserId(), dto))).ToActionResult();

    [HttpPatch("{id}/increment")]
    public async Task<IActionResult> Increment(int id)
        => (await _increment.HandleAsync(new IncrementStreakRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpPatch("{id}/reset")]
    public async Task<IActionResult> Reset(int id)
        => (await _reset.HandleAsync(new ResetStreakRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => (await _delete.HandleAsync(new DeleteStreakRequest(id, GetCurrentUserId()))).ToActionResult();
}
