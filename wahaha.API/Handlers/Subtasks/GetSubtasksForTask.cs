using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Subtasks;

public sealed record GetSubtasksForTaskRequest(Guid TaskId, Guid UserId);

public sealed class GetSubtasksForTaskHandler : IRequestHandler<GetSubtasksForTaskRequest, IEnumerable<SubtaskDto>>
{
    private readonly ISubtaskRepository _subtaskRepo;
    private readonly ITaskRepository _taskRepo;
    private readonly IMapper _mapper;

    public GetSubtasksForTaskHandler(ISubtaskRepository subtaskRepo, ITaskRepository taskRepo, IMapper mapper)
    {
        _subtaskRepo = subtaskRepo;
        _taskRepo = taskRepo;
        _mapper = mapper;
    }

    public async Task<HandlerResult<IEnumerable<SubtaskDto>>> HandleAsync(GetSubtasksForTaskRequest request, CancellationToken ct = default)
    {
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
            return HandlerResult<IEnumerable<SubtaskDto>>.NotFound($"Task {request.TaskId} not found.");

        var subtasks = await _subtaskRepo.GetByTaskIdAsync(request.TaskId);
        return HandlerResult<IEnumerable<SubtaskDto>>.Ok(_mapper.Map<IEnumerable<SubtaskDto>>(subtasks));
    }
}
