using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record LogCounterRequest(Guid TaskId, Guid UserId, DateTime ClientToday, CheckInRequest Request);

public sealed class LogCounterHandler : IRequestHandler<LogCounterRequest, CheckInCycleDto>
{
    private readonly ITaskRepository _taskRepo;
    private readonly ITaskCheckInCycleRepository _cycleRepo;
    private readonly IMapper _mapper;

    public LogCounterHandler(ITaskRepository taskRepo, ITaskCheckInCycleRepository cycleRepo, IMapper mapper)
    {
        _taskRepo = taskRepo;
        _cycleRepo = cycleRepo;
        _mapper = mapper;
    }

    public async Task<HandlerResult<CheckInCycleDto>> HandleAsync(LogCounterRequest request, CancellationToken ct = default)
    {
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
            return HandlerResult<CheckInCycleDto>.NotFound($"Task with ID {request.TaskId} was not found.");
        if (!task.IsRecurring)
            return HandlerResult<CheckInCycleDto>.BadRequest("Only recurring tasks can log a counter.");
        if (!task.HasCounter)
            return HandlerResult<CheckInCycleDto>.BadRequest("This task does not track a counter.");
        if (!request.Request.CounterValue.HasValue)
            return HandlerResult<CheckInCycleDto>.BadRequest("Counter value is required.");

        // Once today's check-in is committed the cycle is closed.
        if (task.LastCheckInDate.HasValue && task.DueDate.HasValue)
        {
            var cycleStart = TaskCycleHelpers.GetCycleStart(task.DueDate.Value.Date, task.RecurrenceRule);
            if (task.LastCheckInDate.Value.Date > cycleStart)
                return HandlerResult<CheckInCycleDto>.BadRequest("Already checked in for this cycle — logs are closed.");
        }

        var counterValue = request.Request.CounterValue.Value;
        if (counterValue < 0)
        {
            var dayTotal = await _cycleRepo.GetDailyCounterSumAsync(request.TaskId, request.ClientToday);
            if (dayTotal + counterValue < 0)
                return HandlerResult<CheckInCycleDto>.BadRequest("Counter total for the day cannot go below zero.");
        }
        else if (task.CapLogAtGoal && task.CounterGoal.HasValue && counterValue > 0)
        {
            var dayTotal = await _cycleRepo.GetDailyCounterSumAsync(request.TaskId, request.ClientToday);
            if (dayTotal + counterValue > task.CounterGoal.Value)
                return HandlerResult<CheckInCycleDto>.BadRequest($"Counter is capped at the goal of {task.CounterGoal.Value}.");
        }

        var cycle = await _cycleRepo.CreateAsync(new TaskCheckInCycle
        {
            TaskId = request.TaskId,
            CheckInDate = request.ClientToday,
            CounterValue = counterValue,
            CreatedAt = DateTime.UtcNow,
            CycleType = "log",
        });
        return HandlerResult<CheckInCycleDto>.Ok(_mapper.Map<CheckInCycleDto>(cycle));
    }
}
