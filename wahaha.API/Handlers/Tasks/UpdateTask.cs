using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;
using DomainTask = wahaha.API.Models.Domain.Task;

namespace wahaha.API.Handlers.Tasks;

public sealed record UpdateTaskRequest(Guid TaskId, Guid UserId, DateTime ClientToday, UpdateTaskDto Dto);

public sealed class UpdateTaskHandler : IRequestHandler<UpdateTaskRequest, Unit>
{
    private readonly ITaskRepository _taskRepo;
    private readonly ITaskPenaltyService _penalty;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateTaskHandler> _logger;

    public UpdateTaskHandler(
        ITaskRepository taskRepo,
        ITaskPenaltyService penalty,
        IMapper mapper,
        ILogger<UpdateTaskHandler> logger)
    {
        _taskRepo = taskRepo;
        _penalty = penalty;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(UpdateTaskRequest request, CancellationToken ct = default)
    {
        var dto = request.Dto;
        if (request.TaskId != dto.TaskId)
            return HandlerResult<Unit>.BadRequest("Task ID in the URL does not match the request body.");

        _logger.LogInformation("Updating task {TaskId}", request.TaskId);
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for update", request.TaskId);
            return HandlerResult<Unit>.NotFound($"Task with ID {request.TaskId} was not found.");
        }

        if (dto.DueDate.HasValue
            && dto.DueDate.Value.Date < request.ClientToday
            && dto.DueDate.Value.Date != task.DueDate?.Date)
            return HandlerResult<Unit>.BadRequest("Due date cannot be set to a past date.");

        if ((DateTime.UtcNow - task.CreatedAt).TotalHours > 24)
        {
            if (!string.Equals(dto.Title?.Trim(), task.Title?.Trim(), StringComparison.Ordinal))
                return HandlerResult<Unit>.BadRequest("Task title cannot be changed more than 24 hours after creation.");
            if (dto.PointValue != task.PointValue)
                return HandlerResult<Unit>.BadRequest("Task point value cannot be changed more than 24 hours after creation.");
        }

        if (dto.PointValue != task.PointValue || !string.Equals(dto.Category, task.Category, StringComparison.OrdinalIgnoreCase))
        {
            var perTaskCap = Models.PointCaps.MaxFor(dto.Category);
            if (dto.PointValue > perTaskCap)
                return HandlerResult<Unit>.BadRequest($"{dto.Category} tasks are capped at {perTaskCap} points each.");
        }

        if (Enum.TryParse<ByteTaskStatus>(dto.Status, true, out var resultingStatus))
        {
            var candidate = new DomainTask
            {
                Status = resultingStatus,
                IsRecurring = dto.IsRecurring,
                DueDate = dto.DueDate ?? task.DueDate,
            };
            if (_penalty.ShouldPenalize(candidate, request.ClientToday))
                return HandlerResult<Unit>.BadRequest(
                    $"Cannot keep this task in progress — its due date is more than {_penalty.OverdueThresholdDays - 1} days in the past. Reschedule it first.");
        }

        _mapper.Map(dto, task);
        if (task.WasPenalized && task.DueDate.HasValue && task.DueDate.Value.Date >= request.ClientToday)
            task.WasPenalized = false;
        await _taskRepo.UpdateAsync(task);

        _logger.LogInformation("Task {TaskId} updated successfully", request.TaskId);
        return HandlerResult<Unit>.NoContent();
    }
}
