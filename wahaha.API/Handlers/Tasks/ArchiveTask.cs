using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record ArchiveTaskRequest(Guid TaskId, Guid UserId);

public sealed class ArchiveTaskHandler : IRequestHandler<ArchiveTaskRequest, Unit>
{
    private readonly ITaskRepository _taskRepo;
    private readonly ILogger<ArchiveTaskHandler> _logger;

    public ArchiveTaskHandler(ITaskRepository taskRepo, ILogger<ArchiveTaskHandler> logger)
    {
        _taskRepo = taskRepo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(ArchiveTaskRequest request, CancellationToken ct = default)
    {
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for archive", request.TaskId);
            return HandlerResult<Unit>.NotFound($"Task with ID {request.TaskId} was not found.");
        }
        if (task.Status != ByteTaskStatus.completed)
            return HandlerResult<Unit>.BadRequest("Only completed tasks can be archived.");
        // Non-recurring tasks earn their points on submit, so block archiving until the
        // points have been claimed — otherwise archiving silently forfeits them. Recurring
        // tasks earn via check-in (Submitted is never set / reset each cycle), so they are exempt.
        if (!task.IsRecurring && task.Submitted != true)
            return HandlerResult<Unit>.BadRequest("Submit this task for points before archiving it.");
        var success = await _taskRepo.SetArchivedAsync(request.TaskId, true);
        if (!success) return HandlerResult<Unit>.BadRequest("Task could not be archived.");
        _logger.LogInformation("Task {TaskId} archived by user {UserId}", request.TaskId, request.UserId);
        return HandlerResult<Unit>.NoContent();
    }
}
