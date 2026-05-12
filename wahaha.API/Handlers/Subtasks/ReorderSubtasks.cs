using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Subtasks;

public sealed record ReorderSubtasksRequestModel(Guid TaskId, Guid UserId, ReorderSubtasksRequest Request);

public sealed class ReorderSubtasksHandler : IRequestHandler<ReorderSubtasksRequestModel, Unit>
{
    private readonly ISubtaskRepository _subtaskRepo;
    private readonly ITaskRepository _taskRepo;

    public ReorderSubtasksHandler(ISubtaskRepository subtaskRepo, ITaskRepository taskRepo)
    {
        _subtaskRepo = subtaskRepo;
        _taskRepo = taskRepo;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(ReorderSubtasksRequestModel request, CancellationToken ct = default)
    {
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
            return HandlerResult<Unit>.NotFound($"Task {request.TaskId} not found.");

        var subtasks = (await _subtaskRepo.GetByTaskIdAsync(request.TaskId)).ToList();
        var byId = subtasks.ToDictionary(s => s.SubtaskId);
        for (int i = 0; i < request.Request.OrderedIds.Count; i++)
        {
            if (!byId.TryGetValue(request.Request.OrderedIds[i], out var s)) continue;
            s.SortOrder = i;
            await _subtaskRepo.UpdateAsync(s);
        }
        return HandlerResult<Unit>.NoContent();
    }
}
