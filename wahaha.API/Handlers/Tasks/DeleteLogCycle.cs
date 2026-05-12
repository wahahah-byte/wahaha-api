using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record DeleteLogCycleRequest(Guid TaskId, int CycleId, Guid UserId);

public sealed class DeleteLogCycleHandler : IRequestHandler<DeleteLogCycleRequest, Unit>
{
    private readonly ITaskRepository _taskRepo;
    private readonly ITaskCheckInCycleRepository _cycleRepo;

    public DeleteLogCycleHandler(ITaskRepository taskRepo, ITaskCheckInCycleRepository cycleRepo)
    {
        _taskRepo = taskRepo;
        _cycleRepo = cycleRepo;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(DeleteLogCycleRequest request, CancellationToken ct = default)
    {
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
            return HandlerResult<Unit>.NotFound($"Task with ID {request.TaskId} was not found.");

        var cycle = await _cycleRepo.GetByIdAsync(request.CycleId);
        if (cycle == null || cycle.TaskId != request.TaskId)
            return HandlerResult<Unit>.NotFound($"Check-in cycle {request.CycleId} was not found for task {request.TaskId}.");

        if (cycle.CycleType != "log")
            return HandlerResult<Unit>.BadRequest("This cycle is a check-in and must be reversed via /undo.");

        await _cycleRepo.DeleteAsync(cycle.CycleId);
        return HandlerResult<Unit>.NoContent();
    }
}
