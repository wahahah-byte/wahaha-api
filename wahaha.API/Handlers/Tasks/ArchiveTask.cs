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
        var success = await _taskRepo.SetArchivedAsync(request.TaskId, true);
        if (!success) return HandlerResult<Unit>.BadRequest("Task could not be archived.");
        _logger.LogInformation("Task {TaskId} archived by user {UserId}", request.TaskId, request.UserId);
        return HandlerResult<Unit>.NoContent();
    }
}
