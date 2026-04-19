using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO           ;
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
    private readonly IPointTransactionRepository _pointTransactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStreakRepository _streakRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TasksController> _logger;

    private const int RecurringDailyPointCap = 50;

    public TasksController(
        ITaskRepository taskRepository,
        IPointTransactionRepository pointTransactionRepository,
        IUserRepository userRepository,
        IStreakRepository streakRepository,
        IMapper mapper,
        ILogger<TasksController> logger)
    {
        _taskRepository = taskRepository;
        _pointTransactionRepository = pointTransactionRepository;
        _userRepository = userRepository;
        _streakRepository = streakRepository;
        _mapper = mapper;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
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
        task.Status = ByteTaskStatus.pending; // always starts as pending
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

    [HttpPatch("{id}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        _logger.LogInformation("Starting task {TaskId}", id);
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for start", id);
            return NotFound($"Task with ID {id} was not found.");
        }

        if (task.Status != ByteTaskStatus.pending)
            return BadRequest($"Task cannot be started — current status is {task.Status}. Only pending tasks can be started.");

        var success = await _taskRepository.StartAsync(id);

        if (!success)
            return BadRequest("Task could not be started.");

        _logger.LogInformation("Task {TaskId} started successfully", id);
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

        if (task.Status == ByteTaskStatus.completed)
            return BadRequest("Task is already completed.");

        var success = await _taskRepository.CompleteAsync(id);

        if (!success)
            return BadRequest("Task could not be completed.");

        _logger.LogInformation("Task {TaskId} completed successfully", id);
        return NoContent();
    }

    [HttpPost("{id}/checkin")]
    public async Task<ActionResult<CheckInResponse>> CheckIn(Guid id)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} checking in recurring task {TaskId}", userId, id);

        var task = await _taskRepository.GetByIdAsync(id);
        var limit = await CheckPointLimit(userId, 5);

        if (task == null || task.UserId != userId)
            return NotFound($"Task with ID {id} was not found.");

        if (!task.IsRecurring)
            return BadRequest("Only recurring tasks can be checked in.");

        if (task.Status != ByteTaskStatus.pending)
            return BadRequest($"Task must be pending to check in — current status is {task.Status}.");
        if (await CheckPointLimit(userId, 0) >= 50)
        {
            return BadRequest("Daily check-in limit reached.");
        }

        // Award points up to recurring cap
        var alreadyEarned = await _pointTransactionRepository.GetDailyEarnedBySourceTypeAsync(userId, DateTime.UtcNow, SourceType.recurring_task);
        var remaining = RecurringDailyPointCap - alreadyEarned;
        var pointsToAward = Math.Max(0, Math.Min(task.PointValue, remaining));

        if (pointsToAward > 0)
        {
            var transaction = new PointTransaction
            {
                UserId = userId,
                Amount = pointsToAward,
                Type = TransactionType.EARN,
                SourceType = SourceType.recurring_task,
                Description = $"Check-in: {task.Title}",
                CreatedAt = DateTime.UtcNow
            };
            await _pointTransactionRepository.CreateAsync(transaction);
            await _userRepository.AddPointsAsync(userId, pointsToAward);
        }

        var newRecurringTotal = alreadyEarned + pointsToAward;

        // Find or create streak
        var streakType = id.ToString();
        var streak = await _streakRepository.GetByUserAndTypeAsync(userId, streakType);
        var streakReset = false;

        if (streak == null)
        {
            streak = await _streakRepository.CreateAsync(new Streak
            {
                UserId = userId,
                StreakType = streakType,
                CurrentCount = 0,
                LongestCount = 0,
                BonusMultiplier = 1.0m,
                IsActive = true,
                LastActivityDate = DateTime.UtcNow
            });
        }
        else
        {
            // Reset if missed a cycle
            var maxGapDays = task.RecurrenceRule switch
            {
                "daily"    => 1,
                "weekdays" => 3,
                "weekly"   => 7,
                "biweekly" => 14,
                "monthly"  => 31,
                _          => 1
            };
            var daysSinceLast = (DateTime.UtcNow - streak.LastActivityDate).TotalDays;
            if (daysSinceLast > maxGapDays)
            {
                await _streakRepository.ResetAsync(streak.StreakId);
                streakReset = true;
            }
        }

        await _streakRepository.IncrementAsync(streak.StreakId);
        var updatedStreak = await _streakRepository.GetByIdAsync(streak.StreakId);

        // Advance to next due date and reset to pending
        var nextDue = ComputeNextDueDate(task.DueDate, task.RecurrenceRule);
        task.Status = ByteTaskStatus.pending;
        task.DueDate = nextDue;
        task.CompletedAt = null;
        task.Submitted = false;
        await _taskRepository.UpdateAsync(task);

        var user = await _userRepository.GetByIdAsync(userId);

        _logger.LogInformation("Check-in complete for task {TaskId}: {Points} pts, streak {Count}", id, pointsToAward, updatedStreak?.CurrentCount);
        return Ok(new CheckInResponse
        {
            PointsAwarded = pointsToAward,
            NewBalance = user?.CurrentBalance ?? 0,
            RecurringDailyTotal = newRecurringTotal,
            StreakCount = updatedStreak?.CurrentCount ?? 1,
            LongestCount = updatedStreak?.LongestCount ?? 1,
            BonusMultiplier = updatedStreak?.BonusMultiplier ?? 1.0m,
            StreakReset = streakReset,
            NextDueDate = nextDue?.ToString("yyyy-MM-dd") ?? string.Empty
        });
    }
    public async Task<int> CheckPointLimit(Guid userId, int pointsToAdd)
    {
        var today = DateTime.UtcNow.Date;

        var pointsToday = await _pointTransactionRepository.GetDailyEarnedAsync(userId, today);

        return (pointsToday);
    }
    private static DateTime? ComputeNextDueDate(DateTime? dueDate, string? rule)
    {
        var base_ = dueDate ?? DateTime.UtcNow;
        base_ = new DateTime(base_.Year, base_.Month, base_.Day, 12, 0, 0, DateTimeKind.Utc);
        switch (rule)
        {
            case "daily":    base_ = base_.AddDays(1); break;
            case "weekdays":
                base_ = base_.AddDays(1);
                while (base_.DayOfWeek == DayOfWeek.Saturday || base_.DayOfWeek == DayOfWeek.Sunday)
                    base_ = base_.AddDays(1);
                break;
            case "weekly":   base_ = base_.AddDays(7);  break;
            case "biweekly": base_ = base_.AddDays(14); break;
            case "monthly":  base_ = base_.AddMonths(1); break;
        }
        return base_;
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
