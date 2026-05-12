using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record UpdateCheckInCycleRequest(Guid TaskId, int CycleId, Guid UserId, CheckInRequest Request);

public sealed class UpdateCheckInCycleHandler : IRequestHandler<UpdateCheckInCycleRequest, Unit>
{
    private readonly ITaskRepository _taskRepo;
    private readonly ITaskCheckInCycleRepository _cycleRepo;

    public UpdateCheckInCycleHandler(ITaskRepository taskRepo, ITaskCheckInCycleRepository cycleRepo)
    {
        _taskRepo = taskRepo;
        _cycleRepo = cycleRepo;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(UpdateCheckInCycleRequest request, CancellationToken ct = default)
    {
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
            return HandlerResult<Unit>.NotFound($"Task with ID {request.TaskId} was not found.");
        if (!task.HasCounter)
            return HandlerResult<Unit>.BadRequest("This task does not track a counter.");
        if (request.Request.CounterValue.HasValue && request.Request.CounterValue.Value < 0)
            return HandlerResult<Unit>.BadRequest("Counter value must be non-negative.");

        var cycle = await _cycleRepo.GetByIdAsync(request.CycleId);
        if (cycle == null || cycle.TaskId != request.TaskId)
            return HandlerResult<Unit>.NotFound($"Check-in cycle {request.CycleId} was not found for task {request.TaskId}.");

        cycle.CounterValue = request.Request.CounterValue;
        await _cycleRepo.UpdateAsync(cycle);
        return HandlerResult<Unit>.NoContent();
    }
}
