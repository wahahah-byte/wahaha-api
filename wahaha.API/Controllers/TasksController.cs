using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<TaskDto>>(tasks));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null)
            return NotFound($"Task with ID {id} was not found.");

        return Ok(_mapper.Map<TaskDto>(task));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetByUser(Guid userId)
    {
        var tasks = await _taskRepository.GetByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<TaskDto>>(tasks));
    }

    [HttpGet("user/{userId}/pending")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetPendingByUser(Guid userId)
    {
        var tasks = await _taskRepository.GetPendingByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<TaskDto>>(tasks));
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var task = _mapper.Map<Models.Domain.Task>(dto);
        var created = await _taskRepository.CreateAsync(task);

        return CreatedAtAction(nameof(GetById), new { id = created.TaskId }, _mapper.Map<TaskDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskDto dto)
    {
        if (id != dto.TaskId)
            return BadRequest("Task ID in the URL does not match the request body.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null)
            return NotFound($"Task with ID {id} was not found.");

        _mapper.Map(dto, task);
        await _taskRepository.UpdateAsync(task);

        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var success = await _taskRepository.CompleteAsync(id);

        if (!success)
            return NotFound($"Task with ID {id} was not found or is already completed.");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _taskRepository.DeleteAsync(id);

        if (!success)
            return NotFound($"Task with ID {id} was not found.");

        return NoContent();
    }
}
