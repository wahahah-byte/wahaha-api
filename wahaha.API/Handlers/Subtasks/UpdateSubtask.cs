using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Subtasks;

public sealed record UpdateSubtaskRequestModel(int SubtaskId, Guid UserId, UpdateSubtaskRequest Request);

public sealed class UpdateSubtaskHandler : IRequestHandler<UpdateSubtaskRequestModel, SubtaskDto>
{
    private readonly ISubtaskRepository _subtaskRepo;
    private readonly ITaskRepository _taskRepo;
    private readonly IMapper _mapper;

    public UpdateSubtaskHandler(ISubtaskRepository subtaskRepo, ITaskRepository taskRepo, IMapper mapper)
    {
        _subtaskRepo = subtaskRepo;
        _taskRepo = taskRepo;
        _mapper = mapper;
    }

    public async Task<HandlerResult<SubtaskDto>> HandleAsync(UpdateSubtaskRequestModel request, CancellationToken ct = default)
    {
        var subtask = await _subtaskRepo.GetByIdAsync(request.SubtaskId);
        if (subtask == null) return HandlerResult<SubtaskDto>.NotFound($"Subtask {request.SubtaskId} not found.");
        var task = await _taskRepo.GetByIdAsync(subtask.TaskId);
        if (task == null || task.UserId != request.UserId)
            return HandlerResult<SubtaskDto>.NotFound($"Subtask {request.SubtaskId} not found.");

        var r = request.Request;
        if (r.Title != null)
        {
            var trimmed = r.Title.Trim();
            if (trimmed.Length == 0) return HandlerResult<SubtaskDto>.BadRequest("Title cannot be empty.");
            subtask.Title = trimmed;
        }
        if (r.Completed.HasValue) subtask.Completed = r.Completed.Value;
        if (r.SortOrder.HasValue) subtask.SortOrder = r.SortOrder.Value;
        if (r.SetsTarget.HasValue) subtask.SetsTarget = r.SetsTarget.Value;
        if (r.RepsTarget.HasValue) subtask.RepsTarget = r.RepsTarget.Value;
        if (r.SetsCompleted.HasValue) subtask.SetsCompleted = r.SetsCompleted.Value;

        await _subtaskRepo.UpdateAsync(subtask);
        return HandlerResult<SubtaskDto>.Ok(_mapper.Map<SubtaskDto>(subtask));
    }
}
