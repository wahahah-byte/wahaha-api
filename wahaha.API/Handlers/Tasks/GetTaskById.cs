using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record GetTaskByIdRequest(Guid TaskId, Guid UserId);

public sealed class GetTaskByIdHandler : IRequestHandler<GetTaskByIdRequest, TaskDto>
{
    private readonly ITaskRepository _taskRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTaskByIdHandler> _logger;

    public GetTaskByIdHandler(ITaskRepository taskRepo, IMapper mapper, ILogger<GetTaskByIdHandler> logger)
    {
        _taskRepo = taskRepo;
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
        return HandlerResult<TaskDto>.Ok(_mapper.Map<TaskDto>(task));
    }
}
