using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record UnarchiveTaskRequest(Guid TaskId, Guid UserId);

public sealed class UnarchiveTaskHandler : IRequestHandler<UnarchiveTaskRequest, Unit>
{
    private readonly ITaskRepository _taskRepo;
    private readonly ILogger<UnarchiveTaskHandler> _logger;

    public UnarchiveTaskHandler(ITaskRepository taskRepo, ILogger<UnarchiveTaskHandler> logger)
    {
        _taskRepo = taskRepo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(UnarchiveTaskRequest request, CancellationToken ct = default)
    {
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for unarchive", request.TaskId);
            return HandlerResult<Unit>.NotFound($"Task with ID {request.TaskId} was not found.");
        }
        var success = await _taskRepo.SetArchivedAsync(request.TaskId, false);
        if (!success) return HandlerResult<Unit>.BadRequest("Task could not be unarchived.");
        _logger.LogInformation("Task {TaskId} unarchived by user {UserId}", request.TaskId, request.UserId);
        return HandlerResult<Unit>.NoContent();
    }
}
