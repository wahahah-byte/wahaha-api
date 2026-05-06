using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
public class SubtasksController : ControllerBase
{
    private readonly ISubtaskRepository _subtaskRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SubtasksController> _logger;

    public SubtasksController(
        ISubtaskRepository subtaskRepository,
        ITaskRepository taskRepository,
        IMapper mapper,
        ILogger<SubtasksController> logger)
    {
        _subtaskRepository = subtaskRepository;
        _taskRepository = taskRepository;
        _mapper = mapper;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    private async Task<Models.Domain.Task?> GetOwnedTaskAsync(Guid taskId)
    {
        var userId = GetCurrentUserId();
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null || task.UserId != userId) return null;
        return task;
    }

    private async Task<Subtask?> GetOwnedSubtaskAsync(int subtaskId)
    {
        var subtask = await _subtaskRepository.GetByIdAsync(subtaskId);
        if (subtask == null) return null;
        var task = await GetOwnedTaskAsync(subtask.TaskId);
        return task == null ? null : subtask;
    }

    [HttpGet("api/tasks/{taskId:guid}/subtasks")]
    public async Task<ActionResult<IEnumerable<SubtaskDto>>> GetForTask(Guid taskId)
    {
        var task = await GetOwnedTaskAsync(taskId);
        if (task == null) return NotFound($"Task {taskId} not found.");

        var subtasks = await _subtaskRepository.GetByTaskIdAsync(taskId);
        return Ok(_mapper.Map<IEnumerable<SubtaskDto>>(subtasks));
    }

    [HttpPost("api/tasks/{taskId:guid}/subtasks")]
    public async Task<ActionResult<SubtaskDto>> Create(Guid taskId, [FromBody] CreateSubtaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required.");

        var task = await GetOwnedTaskAsync(taskId);
        if (task == null) return NotFound($"Task {taskId} not found.");

        var sortOrder = await _subtaskRepository.GetNextSortOrderAsync(taskId);
        var subtask = new Subtask
        {
            TaskId = taskId,
            Title = request.Title.Trim(),
            Completed = false,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _subtaskRepository.CreateAsync(subtask);
        _logger.LogInformation("Created subtask {SubtaskId} on task {TaskId}", created.SubtaskId, taskId);
        return Ok(_mapper.Map<SubtaskDto>(created));
    }

    [HttpPatch("api/subtasks/{id:int}")]
    public async Task<ActionResult<SubtaskDto>> Update(int id, [FromBody] UpdateSubtaskRequest request)
    {
        var subtask = await GetOwnedSubtaskAsync(id);
        if (subtask == null) return NotFound($"Subtask {id} not found.");

        if (request.Title != null)
        {
            var trimmed = request.Title.Trim();
            if (trimmed.Length == 0) return BadRequest("Title cannot be empty.");
            subtask.Title = trimmed;
        }
        if (request.Completed.HasValue) subtask.Completed = request.Completed.Value;
        if (request.SortOrder.HasValue) subtask.SortOrder = request.SortOrder.Value;

        await _subtaskRepository.UpdateAsync(subtask);
        return Ok(_mapper.Map<SubtaskDto>(subtask));
    }

    [HttpDelete("api/subtasks/{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var subtask = await GetOwnedSubtaskAsync(id);
        if (subtask == null) return NotFound($"Subtask {id} not found.");

        await _subtaskRepository.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("api/tasks/{taskId:guid}/subtasks/reorder")]
    public async Task<ActionResult> Reorder(Guid taskId, [FromBody] ReorderSubtasksRequest request)
    {
        var task = await GetOwnedTaskAsync(taskId);
        if (task == null) return NotFound($"Task {taskId} not found.");

        var subtasks = (await _subtaskRepository.GetByTaskIdAsync(taskId)).ToList();
        var byId = subtasks.ToDictionary(s => s.SubtaskId);
        for (int i = 0; i < request.OrderedIds.Count; i++)
        {
            if (!byId.TryGetValue(request.OrderedIds[i], out var s)) continue;
            s.SortOrder = i;
            await _subtaskRepository.UpdateAsync(s);
        }
        return NoContent();
    }
}
