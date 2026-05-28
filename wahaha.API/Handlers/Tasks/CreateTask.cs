using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using DomainTask = wahaha.API.Models.Domain.Task;

namespace wahaha.API.Handlers.Tasks;

public sealed record CreateTaskRequest(Guid UserId, DateTime ClientToday, CreateTaskDto Dto);

public sealed class CreateTaskHandler : IRequestHandler<CreateTaskRequest, TaskDto>
{
    private readonly ITaskRepository _taskRepo;
    private readonly IStreakRepository _streakRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateTaskHandler> _logger;

    public CreateTaskHandler(
        ITaskRepository taskRepo,
        IStreakRepository streakRepo,
        IMapper mapper,
        ILogger<CreateTaskHandler> logger)
    {
        _taskRepo = taskRepo;
        _streakRepo = streakRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<TaskDto>> HandleAsync(CreateTaskRequest request, CancellationToken ct = default)
    {
        var dto = request.Dto;
        _logger.LogInformation("Creating task for user {UserId}", request.UserId);

        if (dto.DueDate.HasValue && dto.DueDate.Value.Date < request.ClientToday)
            return HandlerResult<TaskDto>.BadRequest("Due date cannot be in the past.");

        var perTaskCap = Models.PointCaps.MaxFor(dto.Category);
        if (dto.PointValue > perTaskCap)
            return HandlerResult<TaskDto>.BadRequest($"{dto.Category} tasks are capped at {perTaskCap} points each.");
        if (dto.IsRecurring && dto.PointValue > Models.PointCaps.RecurringPerTask)
            return HandlerResult<TaskDto>.BadRequest($"Recurring tasks are capped at {Models.PointCaps.RecurringPerTask} points each.");

        var task = _mapper.Map<DomainTask>(dto);
        task.UserId = request.UserId;
        task.Status = ByteTaskStatus.pending;
        var created = await _taskRepo.CreateAsync(task);
        var responseDto = _mapper.Map<TaskDto>(created);

        if (created.IsRecurring)
        {
            var streakType = $"{created.RecurrenceRule}_{created.Category}";
            await _streakRepo.CreateAsync(new Streak
            {
                UserId = request.UserId,
                TaskId = created.TaskId,
                StreakType = streakType,
                CurrentCount = 0,
                LongestCount = 0,
                BonusMultiplier = 1.0m,
                IsActive = true,
                LastActivityDate = DateTime.UtcNow,
            });
            responseDto.CurrentStreakCount = 0;
            responseDto.LongestStreakCount = 0;
        }

        _logger.LogInformation("Task {TaskId} created for user {UserId}", created.TaskId, request.UserId);
        return HandlerResult<TaskDto>.Ok(responseDto);
    }
}
