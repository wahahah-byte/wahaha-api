using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskRepository taskRepository, IMapper mapper, ILogger<TasksController> logger)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }
    [AllowAnonymous]
    [HttpGet("debug/config")]
    public IActionResult DebugConfig([FromServices] IConfiguration config)
    {
        var sqlConn = config.GetConnectionString("WahahaByteConnectionString");
        var blobConn = config.GetConnectionString("AzureBlobStorage");

        return Ok(new
        {
            sqlConnLength = sqlConn?.Length ?? 0,
            sqlConnStart = sqlConn?.Substring(0, Math.Min(20, sqlConn?.Length ?? 0)),
            blobConnLength = blobConn?.Length ?? 0,
            blobConnStart = blobConn?.Substring(0, Math.Min(20, blobConn?.Length ?? 0))
        });
    }
    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetAll([FromQuery] TaskFilterParams filters)
    {
        var userId = GetCurrentUserId();
        filters.UserId = userId;
        _logger.LogDebug("Fetching tasks for user {UserId}", userId);

        var result = await _taskRepository.GetFilteredAsync(filters);

        return Ok(new PagedResult<TaskDto>
        {
            Data = _mapper.Map<IEnumerable<TaskDto>>(result.Data),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id)
    {
        _logger.LogDebug("Fetching task {TaskId}", id);
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized", id);
            return NotFound($"Task with ID {id} was not found.");
        }

        return Ok(_mapper.Map<TaskDto>(task));
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetPending()
    {
        var userId = GetCurrentUserId();
        _logger.LogDebug("Fetching pending tasks for user {UserId}", userId);

        var tasks = await _taskRepository.GetPendingByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<TaskDto>>(tasks));
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Creating task for user {UserId}", userId);

        var task = _mapper.Map<Models.Domain.Task>(dto);
        task.UserId = userId;
        var created = await _taskRepository.CreateAsync(task);

        _logger.LogInformation("Task {TaskId} created for user {UserId}", created.TaskId, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.TaskId }, _mapper.Map<TaskDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskDto dto)
    {
        if (id != dto.TaskId)
            return BadRequest("Task ID in the URL does not match the request body.");

        _logger.LogInformation("Updating task {TaskId}", id);
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for update", id);
            return NotFound($"Task with ID {id} was not found.");
        }

        _mapper.Map(dto, task);
        await _taskRepository.UpdateAsync(task);

        _logger.LogInformation("Task {TaskId} updated successfully", id);
        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        _logger.LogInformation("Completing task {TaskId}", id);
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for completion", id);
            return NotFound($"Task with ID {id} was not found.");
        }

        var success = await _taskRepository.CompleteAsync(id);

        if (!success)
            return BadRequest("Task is already completed.");

        _logger.LogInformation("Task {TaskId} completed successfully", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting task {TaskId}", id);
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for deletion", id);
            return NotFound($"Task with ID {id} was not found.");
        }

        await _taskRepository.DeleteAsync(id);
        _logger.LogInformation("Task {TaskId} deleted successfully", id);
        return NoContent();
    }
}
