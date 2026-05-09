using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO           ;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

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
    private readonly ITaskCheckInCycleRepository _checkInCycleRepository;
    private readonly ITaskPenaltyService _penaltyService;
    private readonly IMapper _mapper;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        ITaskRepository taskRepository,
        IPointTransactionRepository pointTransactionRepository,
        IUserRepository userRepository,
        IStreakRepository streakRepository,
        ITaskCheckInCycleRepository checkInCycleRepository,
        ITaskPenaltyService penaltyService,
        IMapper mapper,
        ILogger<TasksController> logger)
    {
        _taskRepository = taskRepository;
        _pointTransactionRepository = pointTransactionRepository;
        _userRepository = userRepository;
        _streakRepository = streakRepository;
        _checkInCycleRepository = checkInCycleRepository;
        _penaltyService = penaltyService;
        _mapper = mapper;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    private DateTime GetClientToday()
    {
        if (int.TryParse(Request.Headers["X-Timezone-Offset"], out var offsetMinutes))
            return DateTime.UtcNow.AddMinutes(-offsetMinutes).Date;
        return DateTime.UtcNow.Date;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetAll([FromQuery] TaskFilterParams filters)
    {
        var userId = GetCurrentUserId();
        filters.UserId = userId;
        _logger.LogDebug("Fetching tasks for user {UserId}", userId);

        var result = await _taskRepository.GetFilteredAsync(filters);
        var taskList = result.Data.ToList();

        await _penaltyService.ApplyAndPersistAsync(taskList, GetClientToday());

        var dtos = _mapper.Map<List<TaskDto>>(taskList);

        for (var i = 0; i < taskList.Count; i++)
        {
            var streak = taskList[i].Streaks.FirstOrDefault();
            if (streak != null)
            {
                dtos[i].CurrentStreakCount = streak.CurrentCount;
                dtos[i].LongestStreakCount = streak.LongestCount;
            }
        }

        return Ok(new PagedResult<TaskDto>
        {
            Data = dtos,
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

        var clientToday = GetClientToday();
        if (dto.DueDate.HasValue && dto.DueDate.Value.Date < clientToday)
            return BadRequest("Due date cannot be in the past.");

        var perTaskCap = Models.PointCaps.MaxFor(dto.Category);
        if (dto.PointValue > perTaskCap)
            return BadRequest($"{dto.Category} tasks are capped at {perTaskCap} points each.");

        var task = _mapper.Map<Models.Domain.Task>(dto);
        task.UserId = userId;
        task.Status = ByteTaskStatus.pending;
        var created = await _taskRepository.CreateAsync(task);

        var responseDto = _mapper.Map<TaskDto>(created);

        if (created.IsRecurring)
        {
            var streakType = $"{created.RecurrenceRule}_{created.Category}";
            await _streakRepository.CreateAsync(new Streak
            {
                UserId = userId,
                TaskId = created.TaskId,
                StreakType = streakType,
                CurrentCount = 0,
                LongestCount = 0,
                BonusMultiplier = 1.0m,
                IsActive = true,
                LastActivityDate = DateTime.UtcNow
            });
            responseDto.CurrentStreakCount = 0;
            responseDto.LongestStreakCount = 0;
        }

        _logger.LogInformation("Task {TaskId} created for user {UserId}", created.TaskId, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.TaskId }, responseDto);
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

        var clientToday = GetClientToday();
        if (dto.DueDate.HasValue &&
            dto.DueDate.Value.Date < clientToday &&
            dto.DueDate.Value.Date != task.DueDate?.Date)
            return BadRequest("Due date cannot be set to a past date.");

        if ((DateTime.UtcNow - task.CreatedAt).TotalHours > 24)
        {
            if (!string.Equals(dto.Title?.Trim(), task.Title?.Trim(), StringComparison.Ordinal))
                return BadRequest("Task title cannot be changed more than 24 hours after creation.");
            if (dto.PointValue != task.PointValue)
                return BadRequest("Task point value cannot be changed more than 24 hours after creation.");
        }

        if (dto.PointValue != task.PointValue || !string.Equals(dto.Category, task.Category, StringComparison.OrdinalIgnoreCase))
        {
            var perTaskCap = Models.PointCaps.MaxFor(dto.Category);
            if (dto.PointValue > perTaskCap)
                return BadRequest($"{dto.Category} tasks are capped at {perTaskCap} points each.");
        }

        if (Enum.TryParse<ByteTaskStatus>(dto.Status, true, out var resultingStatus))
        {
            var candidate = new Models.Domain.Task
            {
                Status = resultingStatus,
                IsRecurring = dto.IsRecurring,
                DueDate = dto.DueDate ?? task.DueDate,
            };
            if (_penaltyService.ShouldPenalize(candidate, clientToday))
                return BadRequest($"Cannot keep this task in progress — its due date is more than {_penaltyService.OverdueThresholdDays - 1} days in the past. Reschedule it first.");
        }

        _mapper.Map(dto, task);
        if (task.WasPenalized && task.DueDate.HasValue && task.DueDate.Value.Date >= clientToday)
            task.WasPenalized = false;
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

        var clientToday = GetClientToday();
        var startCandidate = new Models.Domain.Task
        {
            Status = ByteTaskStatus.in_progress,
            IsRecurring = task.IsRecurring,
            DueDate = task.DueDate,
        };
        if (_penaltyService.ShouldPenalize(startCandidate, clientToday))
            return BadRequest($"Cannot start this task — its due date is more than {_penaltyService.OverdueThresholdDays - 1} days in the past. Reschedule it first.");

        var success = await _taskRepository.StartAsync(id);

        if (!success)
            return BadRequest("Task could not be started.");

        if (task.WasPenalized)
        {
            task.WasPenalized = false;
            await _taskRepository.UpdateAsync(task);
        }

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
    public async Task<ActionResult<CheckInResponse>> CheckIn(Guid id, [FromBody] CheckInRequest? request = null)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} checking in recurring task {TaskId}", userId, id);

        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != userId)
            return NotFound($"Task with ID {id} was not found.");

        if (!task.IsRecurring)
            return BadRequest("Only recurring tasks can be checked in.");

        if (task.Status != ByteTaskStatus.pending)
            return BadRequest($"Task must be pending to check in — current status is {task.Status}.");

        var counterValue = request?.CounterValue;
        if (counterValue.HasValue)
        {
            if (!task.HasCounter)
                return BadRequest("This task does not track a counter.");
            if (counterValue.Value < 0)
                return BadRequest("Counter value must be non-negative.");
        }

        var clientToday = GetClientToday();

        // Idempotency: refuse a second check-in inside the same cycle.
        // The current cycle is (dueDate - period, dueDate]. If LastCheckInDate
        // falls inside it, the user has already checked in.
        if (task.LastCheckInDate.HasValue && task.DueDate.HasValue)
        {
            var cycleStart = GetCycleStart(task.DueDate.Value.Date, task.RecurrenceRule);
            if (task.LastCheckInDate.Value.Date > cycleStart)
                return BadRequest("Already checked in for this cycle.");
        }

        // Find or create streak by TaskId FK. We do this BEFORE points calc so the
        // post-increment BonusMultiplier can scale this check-in's award (the user
        // sees the tier they just unlocked, not the previous one).
        var streakType = $"{task.RecurrenceRule}_{task.Category}";
        var streak = await _streakRepository.GetByTaskIdAsync(id);
        var streakReset = false;

        if (streak == null)
        {
            streak = await _streakRepository.CreateAsync(new Streak
            {
                UserId = userId,
                TaskId = id,
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
            var daysSinceLast = (clientToday.Date - streak.LastActivityDate.Date).Days;
            if (daysSinceLast > maxGapDays)
            {
                await _streakRepository.ResetAsync(streak.StreakId);
                streakReset = true;
            }
        }

        // Snapshot streak state BEFORE increment so undo can restore it.
        var prevStreakCount = streak.CurrentCount;
        var prevLongestCount = streak.LongestCount;
        var prevStreakLastActivity = streak.LastActivityDate;
        var prevStreakIsActive = streak.IsActive;
        var prevStreakBonusMultiplier = streak.BonusMultiplier;

        await _streakRepository.IncrementAsync(streak.StreakId, clientToday);
        var updatedStreak = await _streakRepository.GetByIdAsync(streak.StreakId);
        var bonusMultiplier = updatedStreak?.BonusMultiplier ?? 1.0m;

        // Award points up to recurring cap and per-category daily cap (recurring bucket).
        // Multiplier is applied to the base point value first, then caps clamp the result —
        // so a bonus can be partially eaten by the cap but never exceeds it.
        var alreadyEarned = await _pointTransactionRepository.GetDailyEarnedBySourceTypeAsync(userId, DateTime.UtcNow, SourceType.recurring_task);
        var alreadyEarnedInCategory = await _pointTransactionRepository.GetDailyEarnedByCategoryAsync(userId, DateTime.UtcNow, task.Category, SourceType.recurring_task);

        if (alreadyEarned >= Models.PointCaps.RecurringDaily)
        {
            return BadRequest("Daily check-in limit reached.");
        }
        var categoryRemaining = Models.PointCaps.PerCategoryRecurringDaily - alreadyEarnedInCategory;
        if (categoryRemaining <= 0)
        {
            return BadRequest($"Daily recurring {task.Category} cap of {Models.PointCaps.PerCategoryRecurringDaily} pts reached.");
        }
        var sourceRemaining = Models.PointCaps.RecurringDaily - alreadyEarned;
        var basePoints = task.PointValue;
        var multipliedPoints = (int)Math.Round(basePoints * (double)bonusMultiplier, MidpointRounding.AwayFromZero);
        var pointsToAward = Math.Max(0, Math.Min(multipliedPoints, Math.Min(sourceRemaining, categoryRemaining)));

        int? pointTransactionId = null;
        if (pointsToAward > 0)
        {
            var transaction = new PointTransaction
            {
                UserId = userId,
                Amount = pointsToAward,
                Type = TransactionType.EARN,
                SourceType = SourceType.recurring_task,
                Category = task.Category,
                Description = bonusMultiplier > 1.0m
                    ? $"Check-in: {task.Title} ({basePoints} × {bonusMultiplier:0.0#}x streak)"
                    : $"Check-in: {task.Title}",
                CreatedAt = DateTime.UtcNow
            };
            await _pointTransactionRepository.CreateAsync(transaction);
            pointTransactionId = transaction.TransactionId;
            await _userRepository.AddPointsAsync(userId, pointsToAward);
        }

        var newRecurringTotal = alreadyEarned + pointsToAward;

        // Snapshot task state BEFORE advance so undo can restore it.
        var prevDueDate = task.DueDate;
        var prevLastCheckInDate = task.LastCheckInDate;

        // Advance to next due date and reset to pending
        var nextDue = ComputeNextDueDate(task.DueDate, task.RecurrenceRule, clientToday);
        task.Status = ByteTaskStatus.pending;
        task.DueDate = nextDue;
        task.CompletedAt = null;
        task.Submitted = false;
        task.LastCheckInDate = clientToday;
        await _taskRepository.UpdateAsync(task);

        var newCycle = await _checkInCycleRepository.CreateAsync(new TaskCheckInCycle
        {
            TaskId = id,
            CheckInDate = clientToday,
            CounterValue = counterValue,
            CreatedAt = DateTime.UtcNow,
            CycleType = "checkin",
            PointsAwarded = pointsToAward,
            PointTransactionId = pointTransactionId,
            PreviousDueDate = prevDueDate,
            PreviousLastCheckInDate = prevLastCheckInDate,
            PreviousStreakCount = prevStreakCount,
            PreviousLongestCount = prevLongestCount,
            PreviousStreakLastActivity = prevStreakLastActivity,
            PreviousStreakIsActive = prevStreakIsActive,
            PreviousStreakBonusMultiplier = prevStreakBonusMultiplier,
        });

        var user = await _userRepository.GetByIdAsync(userId);

        _logger.LogInformation("Check-in complete for task {TaskId}: {Base}×{Mult}={Awarded} pts, streak {Count}", id, basePoints, bonusMultiplier, pointsToAward, updatedStreak?.CurrentCount);
        return Ok(new CheckInResponse
        {
            PointsAwarded = pointsToAward,
            BasePoints = basePoints,
            NewBalance = user?.CurrentBalance ?? 0,
            RecurringDailyTotal = newRecurringTotal,
            StreakCount = updatedStreak?.CurrentCount ?? 1,
            LongestCount = updatedStreak?.LongestCount ?? 1,
            BonusMultiplier = bonusMultiplier,
            StreakReset = streakReset,
            NextDueDate = nextDue?.ToString("yyyy-MM-dd") ?? string.Empty,
            CycleId = newCycle.CycleId,
        });
    }

    [HttpPost("{id}/skip-cycle")]
    public async Task<ActionResult> SkipCycle(Guid id)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} skipping missed cycle for task {TaskId}", userId, id);

        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null || task.UserId != userId)
            return NotFound($"Task with ID {id} was not found.");

        if (!task.IsRecurring || task.RecurrenceRule == null)
            return BadRequest("Only recurring tasks can skip cycles.");

        var streak = await _streakRepository.GetByTaskIdAsync(id);
        if (streak != null)
            await _streakRepository.ResetAsync(streak.StreakId);

        var nextDue = ComputeNextDueDate(task.DueDate, task.RecurrenceRule, GetClientToday());
        task.DueDate = nextDue;
        task.Submitted = false;
        await _taskRepository.UpdateAsync(task);

        _logger.LogInformation("Cycle skipped for task {TaskId}, next due: {NextDue}", id, nextDue);
        return Ok(new { nextDueDate = nextDue?.ToString("yyyy-MM-dd") ?? string.Empty, streakReset = true, streakCount = 0 });
    }

    [HttpPatch("{id}/archive")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var userId = GetCurrentUserId();
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != userId)
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for archive", id);
            return NotFound($"Task with ID {id} was not found.");
        }

        if (task.Status != ByteTaskStatus.completed)
            return BadRequest("Only completed tasks can be archived.");

        var success = await _taskRepository.SetArchivedAsync(id, true);
        if (!success) return BadRequest("Task could not be archived.");

        _logger.LogInformation("Task {TaskId} archived by user {UserId}", id, userId);
        return NoContent();
    }

    [HttpPatch("{id}/unarchive")]
    public async Task<IActionResult> Unarchive(Guid id)
    {
        var userId = GetCurrentUserId();
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != userId)
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for unarchive", id);
            return NotFound($"Task with ID {id} was not found.");
        }

        var success = await _taskRepository.SetArchivedAsync(id, false);
        if (!success) return BadRequest("Task could not be unarchived.");

        _logger.LogInformation("Task {TaskId} unarchived by user {UserId}", id, userId);
        return NoContent();
    }

    [HttpGet("{id}/checkin-history")]
    public async Task<ActionResult<PagedResult<CheckInCycleDto>>> GetCheckInHistory(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 30)
    {
        var userId = GetCurrentUserId();
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null || task.UserId != userId)
            return NotFound($"Task with ID {id} was not found.");

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 30;
        if (pageSize > 100) pageSize = 100;

        var (items, total) = await _checkInCycleRepository.GetByTaskIdAsync(id, pageNumber, pageSize);
        var dtos = _mapper.Map<List<CheckInCycleDto>>(items);
        return Ok(new PagedResult<CheckInCycleDto>
        {
            Data = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        });
    }

    [HttpPost("{id}/log-counter")]
    public async Task<ActionResult<CheckInCycleDto>> LogCounter(Guid id, [FromBody] CheckInRequest request)
    {
        var userId = GetCurrentUserId();
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null || task.UserId != userId)
            return NotFound($"Task with ID {id} was not found.");

        if (!task.IsRecurring)
            return BadRequest("Only recurring tasks can log a counter.");
        if (!task.HasCounter)
            return BadRequest("This task does not track a counter.");
        if (!request.CounterValue.HasValue)
            return BadRequest("Counter value is required.");
        // Log entries are deltas — they can be negative so quick-log "−"
        // buttons can subtract from today's running total. Sums are computed
        // client-side, and the UI prevents driving the day-total below zero.

        var cycle = await _checkInCycleRepository.CreateAsync(new TaskCheckInCycle
        {
            TaskId = id,
            CheckInDate = GetClientToday(),
            CounterValue = request.CounterValue.Value,
            CreatedAt = DateTime.UtcNow,
            CycleType = "log",
        });

        return Ok(_mapper.Map<CheckInCycleDto>(cycle));
    }

    [HttpPatch("{taskId}/checkin-history/{cycleId}")]
    public async Task<IActionResult> UpdateCheckInCycle(Guid taskId, int cycleId, [FromBody] CheckInRequest request)
    {
        var userId = GetCurrentUserId();
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null || task.UserId != userId)
            return NotFound($"Task with ID {taskId} was not found.");

        if (!task.HasCounter)
            return BadRequest("This task does not track a counter.");

        if (request.CounterValue.HasValue && request.CounterValue.Value < 0)
            return BadRequest("Counter value must be non-negative.");

        var cycle = await _checkInCycleRepository.GetByIdAsync(cycleId);
        if (cycle == null || cycle.TaskId != taskId)
            return NotFound($"Check-in cycle {cycleId} was not found for task {taskId}.");

        cycle.CounterValue = request.CounterValue;
        await _checkInCycleRepository.UpdateAsync(cycle);
        return NoContent();
    }

    // Undo a check-in cycle: reverses the points/streak/dueDate/lastCheckInDate
    // changes made by the original CheckIn. Only allowed for the latest cycle of
    // type "checkin" whose CheckInDate equals client-today.
    [HttpPost("{taskId}/checkin/{cycleId}/undo")]
    public async Task<ActionResult<UndoCheckInResponse>> UndoCheckIn(Guid taskId, int cycleId)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} undoing check-in cycle {CycleId} on task {TaskId}", userId, cycleId, taskId);

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null || task.UserId != userId)
            return NotFound($"Task with ID {taskId} was not found.");

        var cycle = await _checkInCycleRepository.GetByIdAsync(cycleId);
        if (cycle == null || cycle.TaskId != taskId)
            return NotFound($"Check-in cycle {cycleId} was not found for task {taskId}.");

        if (cycle.CycleType != "checkin")
            return BadRequest("This cycle is not a check-in and cannot be undone via this endpoint.");

        var latest = await _checkInCycleRepository.GetLatestByTaskIdAsync(taskId);
        if (latest == null || latest.CycleId != cycle.CycleId)
            return BadRequest("Only the most recent check-in can be undone.");

        var clientToday = GetClientToday();
        if (cycle.CheckInDate.Date != clientToday.Date)
            return BadRequest("Check-ins can only be undone on the same day they were made.");

        // Reverse the awarded points (delete the transaction, refund balance).
        var pointsRefunded = cycle.PointsAwarded ?? 0;
        if (cycle.PointTransactionId.HasValue)
        {
            await _pointTransactionRepository.DeleteAsync(cycle.PointTransactionId.Value);
        }
        if (pointsRefunded > 0)
        {
            await _userRepository.RefundPointsAsync(userId, pointsRefunded);
        }

        // Restore streak from snapshot.
        var streak = await _streakRepository.GetByTaskIdAsync(taskId);
        if (streak != null)
        {
            streak.CurrentCount = cycle.PreviousStreakCount ?? 0;
            streak.LongestCount = cycle.PreviousLongestCount ?? streak.LongestCount;
            streak.LastActivityDate = cycle.PreviousStreakLastActivity ?? streak.LastActivityDate;
            streak.IsActive = cycle.PreviousStreakIsActive ?? streak.IsActive;
            streak.BonusMultiplier = cycle.PreviousStreakBonusMultiplier ?? streak.BonusMultiplier;
            await _streakRepository.UpdateAsync(streak);
        }

        // Restore task fields from snapshot.
        task.DueDate = cycle.PreviousDueDate;
        task.LastCheckInDate = cycle.PreviousLastCheckInDate;
        await _taskRepository.UpdateAsync(task);

        // Delete the cycle row last so all rollback fields stay readable above.
        await _checkInCycleRepository.DeleteAsync(cycle.CycleId);

        var user = await _userRepository.GetByIdAsync(userId);
        var newRecurringTotal = await _pointTransactionRepository.GetDailyEarnedBySourceTypeAsync(userId, DateTime.UtcNow, SourceType.recurring_task);

        _logger.LogInformation("Check-in cycle {CycleId} undone: refunded {Points} pts, streak restored to {Count}", cycleId, pointsRefunded, streak?.CurrentCount);

        return Ok(new UndoCheckInResponse
        {
            NewBalance = user?.CurrentBalance ?? 0,
            RecurringDailyTotal = newRecurringTotal,
            StreakCount = streak?.CurrentCount ?? 0,
            LongestCount = streak?.LongestCount ?? 0,
            BonusMultiplier = streak?.BonusMultiplier ?? 1.0m,
            PreviousDueDate = cycle.PreviousDueDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            PreviousLastCheckInDate = cycle.PreviousLastCheckInDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            PointsRefunded = pointsRefunded,
        });
    }

    // Delete a "log"-type cycle (no point/streak/dueDate side effects). Logs
    // are pure data points so any owned row can be removed at any time —
    // unlike "checkin" cycles, which must go through /undo and only on the
    // same day so streaks/points stay coherent.
    [HttpDelete("{taskId}/checkin-history/{cycleId}")]
    public async Task<IActionResult> DeleteLogCycle(Guid taskId, int cycleId)
    {
        var userId = GetCurrentUserId();
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null || task.UserId != userId)
            return NotFound($"Task with ID {taskId} was not found.");

        var cycle = await _checkInCycleRepository.GetByIdAsync(cycleId);
        if (cycle == null || cycle.TaskId != taskId)
            return NotFound($"Check-in cycle {cycleId} was not found for task {taskId}.");

        if (cycle.CycleType != "log")
            return BadRequest("This cycle is a check-in and must be reversed via /undo.");

        await _checkInCycleRepository.DeleteAsync(cycle.CycleId);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting task {TaskId}", id);
        var userId = GetCurrentUserId();

        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null || task.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Task {TaskId} not found or unauthorized for deletion", id);
            return NotFound($"Task with ID {id} was not found.");
        }

        await _streakRepository.DeleteByTaskIdAsync(id);
        await _taskRepository.DeleteAsync(id);
        _logger.LogInformation("Task {TaskId} deleted successfully", id);
        return NoContent();
    }
    public async Task<int> CheckPointLimit(Guid userId, int pointsToAdd)
    {
        var today = DateTime.UtcNow.Date;

        var pointsToday = await _pointTransactionRepository.GetDailyEarnedAsync(userId, today);

        return (pointsToday);
    }
    private static DateTime GetCycleStart(DateTime dueDate, string? rule)
    {
        var d = dueDate.Date;
        return rule switch
        {
            "daily"    => d.AddDays(-1),
            "weekdays" => d.AddDays(-1),
            "weekly"   => d.AddDays(-7),
            "biweekly" => d.AddDays(-14),
            "monthly"  => d.AddMonths(-1),
            _          => d.AddDays(-1),
        };
    }

    private static DateTime? ComputeNextDueDate(DateTime? dueDate, string? rule, DateTime clientToday)
    {
        var baseDate = dueDate?.Date ?? clientToday;
        if (baseDate < clientToday) baseDate = clientToday;
        var base_ = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, 12, 0, 0, DateTimeKind.Utc);
        switch (rule)
        {
            case "daily": base_ = base_.AddDays(1); break;
            case "weekdays":
                base_ = base_.AddDays(1);
                while (base_.DayOfWeek == DayOfWeek.Saturday || base_.DayOfWeek == DayOfWeek.Sunday)
                    base_ = base_.AddDays(1);
                break;
            case "weekly": base_ = base_.AddDays(7); break;
            case "biweekly": base_ = base_.AddDays(14); break;
            case "monthly": base_ = base_.AddMonths(1); break;
        }
        return base_;
    }
}
