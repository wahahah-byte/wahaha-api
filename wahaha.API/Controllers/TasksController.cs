using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;

namespace wahaha.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly WahahaDbContext _context;

    public TasksController(WahahaDbContext context)
    {
        _context = context;
    }

    // ============================================================
    //  GET api/tasks
    //  Returns all tasks
    // ============================================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.Domain.Task>>> GetAll()
    {
        var tasks = await _context.Tasks.ToListAsync();
        return Ok(tasks);
    }

    // ============================================================
    //  GET api/tasks/{id}
    //  Returns a single task by ID
    // ============================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<Models.Domain.Task>> GetById(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound($"Task with ID {id} was not found.");

        return Ok(task);
    }

    // ============================================================
    //  GET api/tasks/user/{userId}
    //  Returns all tasks belonging to a specific user
    // ============================================================
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Models.Domain.Task>>> GetByUser(Guid userId)
    {
        var tasks = await _context.Tasks
            .Where(t => t.UserId == userId)
            .ToListAsync();

        return Ok(tasks);
    }

    // ============================================================
    //  GET api/tasks/user/{userId}/pending
    //  Returns all pending tasks for a user ordered by priority
    // ============================================================
    [HttpGet("user/{userId}/pending")]
    public async Task<ActionResult<IEnumerable<Models.Domain.Task>>> GetPendingByUser(Guid userId)
    {
        var tasks = await _context.Tasks
            .Where(t => t.UserId == userId && t.Status == ByteTaskStatus.pending)
            .OrderByDescending(t => t.Priority)
            .ToListAsync();

        return Ok(tasks);
    }

    // ============================================================
    //  POST api/tasks
    //  Creates a new task
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<Models.Domain.Task>> Create(Models.Domain.Task task)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        task.TaskId = Guid.NewGuid();
        task.CreatedAt = DateTime.UtcNow;

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = task.TaskId }, task);
    }

    // ============================================================
    //  PUT api/tasks/{id}
    //  Updates an existing task
    // ============================================================
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, wahaha.API.Models.Domain.Task updatedTask)
    {
        if (id != updatedTask.TaskId)
            return BadRequest("Task ID in the URL does not match the request body.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound($"Task with ID {id} was not found.");

        task.Title = updatedTask.Title;
        task.Description = updatedTask.Description;
        task.Category = updatedTask.Category;
        task.Priority = updatedTask.Priority;
        task.Status = updatedTask.Status;
        task.PointValue = updatedTask.PointValue;
        task.DueDate = updatedTask.DueDate;
        task.CompletedAt = updatedTask.CompletedAt;
        task.IsRecurring = updatedTask.IsRecurring;
        task.RecurrenceRule = updatedTask.RecurrenceRule;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ============================================================
    //  PATCH api/tasks/{id}/complete
    //  Marks a task as completed and stamps the completed_at date
    // ============================================================
    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound($"Task with ID {id} was not found.");

        if (task.Status == ByteTaskStatus.completed)
            return BadRequest("Task is already completed.");

        task.Status = ByteTaskStatus.completed;
        task.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ============================================================
    //  DELETE api/tasks/{id}
    //  Deletes a task
    // ============================================================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound($"Task with ID {id} was not found.");

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}