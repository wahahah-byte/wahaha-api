using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
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

    public TasksController(ITaskRepository taskRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetAll([FromQuery] TaskFilterParams filters)
    {
        filters.UserId = GetCurrentUserId();
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
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != GetCurrentUserId())
            return NotFound($"Task with ID {id} was not found.");

        return Ok(_mapper.Map<TaskDto>(task));
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetPending()
    {
        var tasks = await _taskRepository.GetPendingByUserAsync(GetCurrentUserId());
        return Ok(_mapper.Map<IEnumerable<TaskDto>>(tasks));
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto)
    {
        var task = _mapper.Map<Models.Domain.Task>(dto);
        task.UserId = GetCurrentUserId();
        var created = await _taskRepository.CreateAsync(task);

        return CreatedAtAction(nameof(GetById), new { id = created.TaskId }, _mapper.Map<TaskDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskDto dto)
    {
        if (id != dto.TaskId)
            return BadRequest("Task ID in the URL does not match the request body.");

        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != GetCurrentUserId())
            return NotFound($"Task with ID {id} was not found.");

        _mapper.Map(dto, task);
        await _taskRepository.UpdateAsync(task);

        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != GetCurrentUserId())
            return NotFound($"Task with ID {id} was not found.");

        var success = await _taskRepository.CompleteAsync(id);

        if (!success)
            return BadRequest("Task is already completed.");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != GetCurrentUserId())
            return NotFound($"Task with ID {id} was not found.");

        await _taskRepository.DeleteAsync(id);
        return NoContent();
    }
}
