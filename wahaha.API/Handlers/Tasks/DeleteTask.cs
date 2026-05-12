using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record DeleteTaskRequest(Guid TaskId, Guid UserId);

public sealed class DeleteTaskHandler : IRequestHandler<DeleteTaskRequest, Unit>
{
    private readonly ITaskRepository _taskRepo;
    private readonly IStreakRepository _streakRepo;
    private readonly ILogger<DeleteTaskHandler> _logger;

    public DeleteTaskHandler(ITaskRepository taskRepo, IStreakRepository streakRepo, ILogger<DeleteTaskHandler> logger)
    {
        _taskRepo = taskRepo;
        _streakRepo = streakRepo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(DeleteTaskRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting task {TaskId}", request.TaskId);
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for deletion", request.TaskId);
            return HandlerResult<Unit>.NotFound($"Task with ID {request.TaskId} was not found.");
        }
        await _streakRepo.DeleteByTaskIdAsync(request.TaskId);
        await _taskRepo.DeleteAsync(request.TaskId);
        _logger.LogInformation("Task {TaskId} deleted successfully", request.TaskId);
        return HandlerResult<Unit>.NoContent();
    }
}
