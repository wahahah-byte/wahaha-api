using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;
using DomainTask = wahaha.API.Models.Domain.Task;

namespace wahaha.API.Handlers.Tasks;

public sealed record StartTaskRequest(Guid TaskId, Guid UserId, DateTime ClientToday);

public sealed class StartTaskHandler : IRequestHandler<StartTaskRequest, Unit>
{
    private readonly ITaskRepository _taskRepo;
    private readonly ITaskPenaltyService _penalty;
    private readonly ILogger<StartTaskHandler> _logger;

    public StartTaskHandler(ITaskRepository taskRepo, ITaskPenaltyService penalty, ILogger<StartTaskHandler> logger)
    {
        _taskRepo = taskRepo;
        _penalty = penalty;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(StartTaskRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting task {TaskId}", request.TaskId);
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for start", request.TaskId);
            return HandlerResult<Unit>.NotFound($"Task with ID {request.TaskId} was not found.");
        }

        if (task.Status != ByteTaskStatus.pending)
            return HandlerResult<Unit>.BadRequest($"Task cannot be started — current status is {task.Status}. Only pending tasks can be started.");

        var startCandidate = new DomainTask
        {
            Status = ByteTaskStatus.in_progress,
            IsRecurring = task.IsRecurring,
            DueDate = task.DueDate,
        };
        if (_penalty.ShouldPenalize(startCandidate, request.ClientToday))
            return HandlerResult<Unit>.BadRequest(
                $"Cannot start this task — its due date is more than {_penalty.OverdueThresholdDays - 1} days in the past. Reschedule it first.");

        var success = await _taskRepo.StartAsync(request.TaskId);
        if (!success) return HandlerResult<Unit>.BadRequest("Task could not be started.");

        if (task.WasPenalized)
        {
            task.WasPenalized = false;
            await _taskRepo.UpdateAsync(task);
        }

        _logger.LogInformation("Task {TaskId} started successfully", request.TaskId);
        return HandlerResult<Unit>.NoContent();
    }
}
