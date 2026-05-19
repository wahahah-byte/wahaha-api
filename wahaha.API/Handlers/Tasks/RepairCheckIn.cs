using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;
public sealed record RepairCheckInRequest(Guid TaskId, Guid UserId);

public sealed record RepairCheckInResponse(string? RestoredLastCheckInDate, string? RestoredDueDate);

public sealed class RepairCheckInHandler : IRequestHandler<RepairCheckInRequest, RepairCheckInResponse>
{
    private readonly ITaskRepository _taskRepo;
    private readonly ITaskCheckInCycleRepository _cycleRepo;
    private readonly ILogger<RepairCheckInHandler> _logger;

    public RepairCheckInHandler(
        ITaskRepository taskRepo,
        ITaskCheckInCycleRepository cycleRepo,
        ILogger<RepairCheckInHandler> logger)
    {
        _taskRepo = taskRepo;
        _cycleRepo = cycleRepo;
        _logger = logger;
    }

    public async Task<HandlerResult<RepairCheckInResponse>> HandleAsync(RepairCheckInRequest req, CancellationToken ct = default)
    {
        var task = await _taskRepo.GetByIdAsync(req.TaskId);
        if (task == null || task.UserId != req.UserId)
            return HandlerResult<RepairCheckInResponse>.NotFound($"Task with ID {req.TaskId} was not found.");

        if (!task.LastCheckInDate.HasValue)
            return HandlerResult<RepairCheckInResponse>.BadRequest("Task is not in a checked-in state — nothing to repair.");

        var latestCheckin = await _cycleRepo.GetLatestCheckinByTaskIdAsync(req.TaskId);
        if (latestCheckin != null)
            return HandlerResult<RepairCheckInResponse>.BadRequest("Task has check-in cycles — use the standard undo flow instead.");

        var restoredDueDate = task.DueDate.HasValue
            ? TaskCycleHelpers.GetCycleStart(task.DueDate.Value.Date, task.RecurrenceRule)
            : (DateTime?)null;

        _logger.LogWarning(
            "Repairing stuck task {TaskId}: clearing LastCheckInDate ({Was}) and rolling DueDate {OldDue} -> {NewDue}",
            req.TaskId, task.LastCheckInDate, task.DueDate, restoredDueDate);

        task.LastCheckInDate = null;
        task.DueDate = restoredDueDate;
        task.Submitted = false;
        await _taskRepo.UpdateAsync(task);

        return HandlerResult<RepairCheckInResponse>.Ok(new RepairCheckInResponse(
            RestoredLastCheckInDate: null,
            RestoredDueDate: restoredDueDate?.ToString("yyyy-MM-dd")));
    }
}
