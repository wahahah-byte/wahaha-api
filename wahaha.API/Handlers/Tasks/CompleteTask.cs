using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record CompleteTaskRequest(Guid TaskId, Guid UserId);

public sealed class CompleteTaskHandler : IRequestHandler<CompleteTaskRequest, Unit>
{
    private readonly ITaskRepository _taskRepo;
    private readonly ILogger<CompleteTaskHandler> _logger;

    public CompleteTaskHandler(ITaskRepository taskRepo, ILogger<CompleteTaskHandler> logger)
    {
        _taskRepo = taskRepo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(CompleteTaskRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Completing task {TaskId}", request.TaskId);
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for completion", request.TaskId);
            return HandlerResult<Unit>.NotFound($"Task with ID {request.TaskId} was not found.");
        }
        if (task.Status == ByteTaskStatus.completed)
            return HandlerResult<Unit>.BadRequest("Task is already completed.");
        var success = await _taskRepo.CompleteAsync(request.TaskId);
        if (!success) return HandlerResult<Unit>.BadRequest("Task could not be completed.");
        _logger.LogInformation("Task {TaskId} completed successfully", request.TaskId);
        return HandlerResult<Unit>.NoContent();
    }
}
