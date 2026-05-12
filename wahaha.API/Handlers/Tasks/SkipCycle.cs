using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record SkipCycleRequest(Guid TaskId, Guid UserId, DateTime ClientToday);

public sealed record SkipCycleResponse(string NextDueDate, bool StreakReset, int StreakCount);

public sealed class SkipCycleHandler : IRequestHandler<SkipCycleRequest, SkipCycleResponse>
{
    private readonly ITaskRepository _taskRepo;
    private readonly IStreakRepository _streakRepo;
    private readonly ILogger<SkipCycleHandler> _logger;

    public SkipCycleHandler(ITaskRepository taskRepo, IStreakRepository streakRepo, ILogger<SkipCycleHandler> logger)
    {
        _taskRepo = taskRepo;
        _streakRepo = streakRepo;
        _logger = logger;
    }

    public async Task<HandlerResult<SkipCycleResponse>> HandleAsync(SkipCycleRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("User {UserId} skipping missed cycle for task {TaskId}", request.UserId, request.TaskId);
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
            return HandlerResult<SkipCycleResponse>.NotFound($"Task with ID {request.TaskId} was not found.");
        if (!task.IsRecurring || task.RecurrenceRule == null)
            return HandlerResult<SkipCycleResponse>.BadRequest("Only recurring tasks can skip cycles.");

        var streak = await _streakRepo.GetByTaskIdAsync(request.TaskId);
        if (streak != null) await _streakRepo.ResetAsync(streak.StreakId);

        var nextDue = TaskCycleHelpers.ComputeNextDueDate(task.DueDate, task.RecurrenceRule, request.ClientToday);
        task.DueDate = nextDue;
        task.Submitted = false;
        await _taskRepo.UpdateAsync(task);

        _logger.LogInformation("Cycle skipped for task {TaskId}, next due: {NextDue}", request.TaskId, nextDue);
        return HandlerResult<SkipCycleResponse>.Ok(new SkipCycleResponse(
            nextDue?.ToString("yyyy-MM-dd") ?? string.Empty, true, 0));
    }
}
