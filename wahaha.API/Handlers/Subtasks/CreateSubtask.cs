using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Subtasks;

public sealed record CreateSubtaskRequestModel(Guid TaskId, Guid UserId, CreateSubtaskRequest Request);

public sealed class CreateSubtaskHandler : IRequestHandler<CreateSubtaskRequestModel, SubtaskDto>
{
    private readonly ISubtaskRepository _subtaskRepo;
    private readonly ITaskRepository _taskRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSubtaskHandler> _logger;

    public CreateSubtaskHandler(
        ISubtaskRepository subtaskRepo,
        ITaskRepository taskRepo,
        IMapper mapper,
        ILogger<CreateSubtaskHandler> logger)
    {
        _subtaskRepo = subtaskRepo;
        _taskRepo = taskRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<SubtaskDto>> HandleAsync(CreateSubtaskRequestModel request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Request.Title))
            return HandlerResult<SubtaskDto>.BadRequest("Title is required.");

        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
            return HandlerResult<SubtaskDto>.NotFound($"Task {request.TaskId} not found.");

        var sortOrder = await _subtaskRepo.GetNextSortOrderAsync(request.TaskId);
        var subtask = new Subtask
        {
            TaskId = request.TaskId,
            Title = request.Request.Title.Trim(),
            Completed = false,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow,
            SetsTarget = request.Request.SetsTarget,
            RepsTarget = request.Request.RepsTarget,
            SetsCompleted = request.Request.SetsTarget.HasValue ? 0 : (int?)null,
        };
        var created = await _subtaskRepo.CreateAsync(subtask);
        _logger.LogInformation("Created subtask {SubtaskId} on task {TaskId}", created.SubtaskId, request.TaskId);
        return HandlerResult<SubtaskDto>.Ok(_mapper.Map<SubtaskDto>(created));
    }
}
