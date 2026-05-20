using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.Tasks;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IRequestHandler<GetTaskListRequest, PagedResult<TaskDto>> _list;
    private readonly IRequestHandler<GetTaskByIdRequest, TaskDto> _byId;
    private readonly IRequestHandler<GetPendingTasksRequest, IEnumerable<TaskDto>> _pending;
    private readonly IRequestHandler<CreateTaskRequest, TaskDto> _create;
    private readonly IRequestHandler<UpdateTaskRequest, Unit> _update;
    private readonly IRequestHandler<StartTaskRequest, Unit> _start;
    private readonly IRequestHandler<CompleteTaskRequest, Unit> _complete;
    private readonly IRequestHandler<CheckInTaskRequest, CheckInResponse> _checkIn;
    private readonly IRequestHandler<SkipCycleRequest, SkipCycleResponse> _skipCycle;
    private readonly IRequestHandler<ArchiveTaskRequest, Unit> _archive;
    private readonly IRequestHandler<UnarchiveTaskRequest, Unit> _unarchive;
    private readonly IRequestHandler<GetCheckInHistoryRequest, PagedResult<CheckInCycleDto>> _history;
    private readonly IRequestHandler<LogCounterRequest, CheckInCycleDto> _log;
    private readonly IRequestHandler<UpdateCheckInCycleRequest, Unit> _updateCycle;
    private readonly IRequestHandler<UndoCheckInHandlerRequest, UndoCheckInResponse> _undo;
    private readonly IRequestHandler<RepairCheckInRequest, RepairCheckInResponse> _repair;
    private readonly IRequestHandler<DeleteLogCycleRequest, Unit> _deleteLog;
    private readonly IRequestHandler<DeleteTaskRequest, Unit> _delete;

    public TasksController(
        IRequestHandler<GetTaskListRequest, PagedResult<TaskDto>> list,
        IRequestHandler<GetTaskByIdRequest, TaskDto> byId,
        IRequestHandler<GetPendingTasksRequest, IEnumerable<TaskDto>> pending,
        IRequestHandler<CreateTaskRequest, TaskDto> create,
        IRequestHandler<UpdateTaskRequest, Unit> update,
        IRequestHandler<StartTaskRequest, Unit> start,
        IRequestHandler<CompleteTaskRequest, Unit> complete,
        IRequestHandler<CheckInTaskRequest, CheckInResponse> checkIn,
        IRequestHandler<SkipCycleRequest, SkipCycleResponse> skipCycle,
        IRequestHandler<ArchiveTaskRequest, Unit> archive,
        IRequestHandler<UnarchiveTaskRequest, Unit> unarchive,
        IRequestHandler<GetCheckInHistoryRequest, PagedResult<CheckInCycleDto>> history,
        IRequestHandler<LogCounterRequest, CheckInCycleDto> log,
        IRequestHandler<UpdateCheckInCycleRequest, Unit> updateCycle,
        IRequestHandler<UndoCheckInHandlerRequest, UndoCheckInResponse> undo,
        IRequestHandler<RepairCheckInRequest, RepairCheckInResponse> repair,
        IRequestHandler<DeleteLogCycleRequest, Unit> deleteLog,
        IRequestHandler<DeleteTaskRequest, Unit> delete)
    {
        _list = list;
        _byId = byId;
        _pending = pending;
        _create = create;
        _update = update;
        _start = start;
        _complete = complete;
        _checkIn = checkIn;
        _skipCycle = skipCycle;
        _archive = archive;
        _unarchive = unarchive;
        _history = history;
        _log = log;
        _updateCycle = updateCycle;
        _undo = undo;
        _repair = repair;
        _deleteLog = deleteLog;
        _delete = delete;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    private DateTime GetClientToday()
    {
        if (int.TryParse(Request.Headers["X-Timezone-Offset"], out var offsetMinutes))
            return DateTime.UtcNow.AddMinutes(-offsetMinutes).Date;
        return DateTime.UtcNow.Date;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetAll([FromQuery] TaskFilterParams filters)
        => (await _list.HandleAsync(new GetTaskListRequest(GetCurrentUserId(), GetClientToday(), filters))).ToActionResult();

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id)
        => (await _byId.HandleAsync(new GetTaskByIdRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetPending()
        => (await _pending.HandleAsync(new GetPendingTasksRequest(GetCurrentUserId()))).ToActionResult();

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto)
    {
        var result = await _create.HandleAsync(new CreateTaskRequest(GetCurrentUserId(), GetClientToday(), dto));
        if (result.Status == HandlerStatus.Ok && result.Value != null)
            return CreatedAtAction(nameof(GetById), new { id = result.Value.TaskId }, result.Value);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskDto dto)
        => (await _update.HandleAsync(new UpdateTaskRequest(id, GetCurrentUserId(), GetClientToday(), dto))).ToActionResult();

    [HttpPatch("{id}/start")]
    public async Task<IActionResult> Start(Guid id)
        => (await _start.HandleAsync(new StartTaskRequest(id, GetCurrentUserId(), GetClientToday()))).ToActionResult();

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
        => (await _complete.HandleAsync(new CompleteTaskRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpPost("{id}/checkin")]
    public async Task<ActionResult<CheckInResponse>> CheckIn(Guid id, [FromBody] CheckInRequest? request = null)
        => (await _checkIn.HandleAsync(new CheckInTaskRequest(id, GetCurrentUserId(), GetClientToday(), request))).ToActionResult();

    [HttpPost("{id}/skip-cycle")]
    public async Task<ActionResult> SkipCycle(Guid id)
        => (await _skipCycle.HandleAsync(new SkipCycleRequest(id, GetCurrentUserId(), GetClientToday()))).ToActionResult();

    [HttpPatch("{id}/archive")]
    public async Task<IActionResult> Archive(Guid id)
        => (await _archive.HandleAsync(new ArchiveTaskRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpPatch("{id}/unarchive")]
    public async Task<IActionResult> Unarchive(Guid id)
        => (await _unarchive.HandleAsync(new UnarchiveTaskRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpGet("{id}/checkin-history")]
    public async Task<ActionResult<PagedResult<CheckInCycleDto>>> GetCheckInHistory(
        Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 30)
        => (await _history.HandleAsync(new GetCheckInHistoryRequest(id, GetCurrentUserId(), pageNumber, pageSize))).ToActionResult();

    [HttpPost("{id}/log-counter")]
    public async Task<ActionResult<CheckInCycleDto>> LogCounter(Guid id, [FromBody] CheckInRequest request)
        => (await _log.HandleAsync(new LogCounterRequest(id, GetCurrentUserId(), GetClientToday(), request))).ToActionResult();

    [HttpPatch("{taskId}/checkin-history/{cycleId}")]
    public async Task<IActionResult> UpdateCheckInCycle(Guid taskId, int cycleId, [FromBody] CheckInRequest request)
        => (await _updateCycle.HandleAsync(new UpdateCheckInCycleRequest(taskId, cycleId, GetCurrentUserId(), request))).ToActionResult();

    [HttpPost("{taskId}/checkin/{cycleId}/undo")]
    public async Task<ActionResult<UndoCheckInResponse>> UndoCheckIn(Guid taskId, int cycleId)
        => (await _undo.HandleAsync(new UndoCheckInHandlerRequest(taskId, cycleId, GetCurrentUserId(), GetClientToday()))).ToActionResult();

    // Recovery endpoint for tasks marked checked but missing a backing cycle row.
    [HttpPost("{taskId}/repair-checkin")]
    public async Task<ActionResult<RepairCheckInResponse>> RepairCheckIn(Guid taskId)
        => (await _repair.HandleAsync(new RepairCheckInRequest(taskId, GetCurrentUserId()))).ToActionResult();

    [HttpDelete("{taskId}/checkin-history/{cycleId}")]
    public async Task<IActionResult> DeleteLogCycle(Guid taskId, int cycleId)
        => (await _deleteLog.HandleAsync(new DeleteLogCycleRequest(taskId, cycleId, GetCurrentUserId()))).ToActionResult();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => (await _delete.HandleAsync(new DeleteTaskRequest(id, GetCurrentUserId()))).ToActionResult();
}
