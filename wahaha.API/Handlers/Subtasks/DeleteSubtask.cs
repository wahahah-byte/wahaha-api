using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Subtasks;

public sealed record DeleteSubtaskRequest(int SubtaskId, Guid UserId);

public sealed class DeleteSubtaskHandler : IRequestHandler<DeleteSubtaskRequest, Unit>
{
    private readonly ISubtaskRepository _subtaskRepo;
    private readonly ITaskRepository _taskRepo;

    public DeleteSubtaskHandler(ISubtaskRepository subtaskRepo, ITaskRepository taskRepo)
    {
        _subtaskRepo = subtaskRepo;
        _taskRepo = taskRepo;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(DeleteSubtaskRequest request, CancellationToken ct = default)
    {
        var subtask = await _subtaskRepo.GetByIdAsync(request.SubtaskId);
        if (subtask == null) return HandlerResult<Unit>.NotFound($"Subtask {request.SubtaskId} not found.");
        var task = await _taskRepo.GetByIdAsync(subtask.TaskId);
        if (task == null || task.UserId != request.UserId)
            return HandlerResult<Unit>.NotFound($"Subtask {request.SubtaskId} not found.");

        await _subtaskRepo.DeleteAsync(request.SubtaskId);
        return HandlerResult<Unit>.NoContent();
    }
}
