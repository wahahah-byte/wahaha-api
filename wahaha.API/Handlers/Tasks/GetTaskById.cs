using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record GetTaskByIdRequest(Guid TaskId, Guid UserId);

public sealed class GetTaskByIdHandler : IRequestHandler<GetTaskByIdRequest, TaskDto>
{
    private readonly ITaskRepository _taskRepo;
    private readonly IStreakRepository _streakRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTaskByIdHandler> _logger;

    public GetTaskByIdHandler(ITaskRepository taskRepo, IStreakRepository streakRepo, IMapper mapper, ILogger<GetTaskByIdHandler> logger)
    {
        _taskRepo = taskRepo;
        _streakRepo = streakRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<TaskDto>> HandleAsync(GetTaskByIdRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching task {TaskId}", request.TaskId);
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized", request.TaskId);
            return HandlerResult<TaskDto>.NotFound($"Task with ID {request.TaskId} was not found.");
        }
        var dto = _mapper.Map<TaskDto>(task);
        var streak = await _streakRepo.GetByTaskIdAsync(request.TaskId);
        // Populate streak counts regardless of IsActive so the tier badge survives undo.
        if (streak != null && streak.UserId == request.UserId)
        {
            dto.CurrentStreakCount = streak.CurrentCount;
            dto.LongestStreakCount = streak.LongestCount;
        }
        return HandlerResult<TaskDto>.Ok(dto);
    }
}
