using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record CheckInTaskRequest(Guid TaskId, Guid UserId, DateTime ClientToday, CheckInRequest? Request);

public sealed class CheckInTaskHandler : IRequestHandler<CheckInTaskRequest, CheckInResponse>
{
    private readonly WahahaDbContext _db;
    private readonly ITaskRepository _taskRepo;
    private readonly IPointTransactionRepository _txRepo;
    private readonly IUserRepository _userRepo;
    private readonly IStreakRepository _streakRepo;
    private readonly ITaskCheckInCycleRepository _cycleRepo;
    private readonly ISubtaskRepository _subtaskRepo;
    private readonly ILogger<CheckInTaskHandler> _logger;

    public CheckInTaskHandler(
        WahahaDbContext db,
        ITaskRepository taskRepo,
        IPointTransactionRepository txRepo,
        IUserRepository userRepo,
        IStreakRepository streakRepo,
        ITaskCheckInCycleRepository cycleRepo,
        ISubtaskRepository subtaskRepo,
        ILogger<CheckInTaskHandler> logger)
    {
        _db = db;
        _taskRepo = taskRepo;
        _txRepo = txRepo;
        _userRepo = userRepo;
        _streakRepo = streakRepo;
        _cycleRepo = cycleRepo;
        _subtaskRepo = subtaskRepo;
        _logger = logger;
    }

    public async Task<HandlerResult<CheckInResponse>> HandleAsync(CheckInTaskRequest req, CancellationToken ct = default)
    {
        // Defensive catch: treat lost concurrency race as "Already checked in".
        try
        {
            return await HandleCore(req, ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Check-in on task {TaskId} lost a concurrency race; reporting Already checked in", req.TaskId);
            return HandlerResult<CheckInResponse>.BadRequest("Already checked in for this cycle.");
        }
    }

    private async Task<HandlerResult<CheckInResponse>> HandleCore(CheckInTaskRequest req, CancellationToken ct)
    {
        // One explicit tx via ExecutionStrategy so concurrent reads see all-or-nothing check-in state.
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            async (token) =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(token);
                var result = await HandleCoreInTx(req, token);
                if (result.Status == HandlerStatus.Ok)
                {
                    await tx.CommitAsync(token);
                }
                // Non-Ok returns leave tx uncommitted; dispose rolls back staged writes.
                return result;
            },
            ct);
    }

    private async Task<HandlerResult<CheckInResponse>> HandleCoreInTx(CheckInTaskRequest req, CancellationToken ct)
    {
        _logger.LogInformation("User {UserId} checking in recurring task {TaskId}", req.UserId, req.TaskId);
        var task = await _taskRepo.GetByIdAsync(req.TaskId);
        if (task == null || task.UserId != req.UserId)
            return HandlerResult<CheckInResponse>.NotFound($"Task with ID {req.TaskId} was not found.");
        if (!task.IsRecurring)
            return HandlerResult<CheckInResponse>.BadRequest("Only recurring tasks can be checked in.");
        if (task.Status != ByteTaskStatus.pending)
            return HandlerResult<CheckInResponse>.BadRequest($"Task must be pending to check in — current status is {task.Status}.");

        var counterValue = req.Request?.CounterValue;
        if (counterValue.HasValue)
        {
            if (!task.HasCounter)
                return HandlerResult<CheckInResponse>.BadRequest("This task does not track a counter.");
            if (counterValue.Value < 0)
                return HandlerResult<CheckInResponse>.BadRequest("Counter value must be non-negative.");
            // Defense-in-depth: clamp at the goal so no codepath persists more than the goal.
            if (task.CapLogAtGoal && task.CounterGoal.HasValue && counterValue.Value > task.CounterGoal.Value)
                counterValue = task.CounterGoal.Value;
        }

        // Idempotency: today < DueDate means already-checked-in for this cycle.
        if (task.LastCheckInDate.HasValue && task.DueDate.HasValue)
        {
            if (req.ClientToday.Date < task.DueDate.Value.Date)
                return HandlerResult<CheckInResponse>.BadRequest("Already checked in for this cycle.");
        }

        // Load user up front; tracked-entity mutations flush in the final SaveChanges.
        var user = await _userRepo.GetByIdAsync(req.UserId);
        if (user == null)
            return HandlerResult<CheckInResponse>.NotFound("User not found.");

        var streakType = $"{task.RecurrenceRule}_{task.Category}";
        var streak = await _streakRepo.GetByTaskIdAsync(req.TaskId);
        var streakReset = false;
        if (streak == null)
        {
            // Stage new streak in the tracker; batched SaveChanges inserts it.
            streak = new Streak
            {
                UserId = req.UserId,
                TaskId = req.TaskId,
                StreakType = streakType,
                CurrentCount = 0,
                LongestCount = 0,
                BonusMultiplier = 1.0m,
                IsActive = true,
                LastActivityDate = DateTime.UtcNow,
            };
            _db.Streaks.Add(streak);
        }
        else
        {
            // Streak resets when task is overdue at check-in (today > DueDate).
            var isOverdue = task.DueDate.HasValue && req.ClientToday.Date > task.DueDate.Value.Date;
            if (isOverdue)
            {
                // Stage reset on tracked entity; SaveChanges flushes with the rest.
                streak.CurrentCount = 0;
                streak.BonusMultiplier = 1.0m;
                streak.IsActive = false;
                streakReset = true;
            }
        }

        var prevStreakCount = streak.CurrentCount;
        var prevLongestCount = streak.LongestCount;
        var prevStreakLastActivity = streak.LastActivityDate;
        var prevStreakIsActive = streak.IsActive;
        var prevStreakBonusMultiplier = streak.BonusMultiplier;

        // Compute post-increment streak state in-memory to avoid an extra fetch.
        var newStreakCount = prevStreakCount + 1;
        var newLongestCount = Math.Max(prevLongestCount, newStreakCount);
        var bonusMultiplier = StreakBonusMultiplier.Compute(newStreakCount);

        // Added or reset paths set values directly; default uses atomic IncrementAsync for concurrency.
        if (_db.Entry(streak).State == EntityState.Added || streakReset)
        {
            streak.CurrentCount = newStreakCount;
            streak.LongestCount = newLongestCount;
            streak.BonusMultiplier = bonusMultiplier;
            streak.LastActivityDate = req.ClientToday;
            streak.IsActive = true;
        }
        else
        {
            await _streakRepo.IncrementAsync(streak.StreakId, req.ClientToday);
        }

        // Combined daily-totals query (single round trip).
        var (alreadyEarned, alreadyEarnedInCategory) = await _txRepo
            .GetDailyEarnedTotalsAsync(req.UserId, DateTime.UtcNow, SourceType.recurring_task, task.Category);

        if (alreadyEarned >= Models.PointCaps.RecurringDaily)
            return HandlerResult<CheckInResponse>.BadRequest("Daily check-in limit reached.");
        var categoryRemaining = Models.PointCaps.PerCategoryRecurringDaily - alreadyEarnedInCategory;
        if (categoryRemaining <= 0)
            return HandlerResult<CheckInResponse>.BadRequest($"Daily recurring {task.Category} cap of {Models.PointCaps.PerCategoryRecurringDaily} pts reached.");
        var sourceRemaining = Models.PointCaps.RecurringDaily - alreadyEarned;
        var basePoints = task.PointValue;
        var multipliedPoints = (int)Math.Round(basePoints * (double)bonusMultiplier, MidpointRounding.AwayFromZero);
        var pointsToAward = Math.Max(0, Math.Min(multipliedPoints, Math.Min(sourceRemaining, categoryRemaining)));

        // Stage point tx ahead of cycle row (cycle's PointTransactionId FK needs the generated id).
        PointTransaction? transaction = null;
        if (pointsToAward > 0)
        {
            transaction = new PointTransaction
            {
                UserId = req.UserId,
                Amount = pointsToAward,
                Type = TransactionType.EARN,
                SourceType = SourceType.recurring_task,
                Category = task.Category,
                Description = bonusMultiplier > 1.0m
                    ? $"Check-in: {task.Title} ({basePoints} × {bonusMultiplier:0.0#}x streak)"
                    : $"Check-in: {task.Title}",
                CreatedAt = DateTime.UtcNow,
            };
            _db.PointTransactions.Add(transaction);
            // Flush tx to read back generated TransactionId; tracked streak changes batch with it.
            await _db.SaveChangesAsync(ct);

            // Mutate user balance in-memory; final SaveChanges picks it up.
            user.CurrentBalance += pointsToAward;
            user.TotalPointsEarned += pointsToAward;
        }

        var newRecurringTotal = alreadyEarned + pointsToAward;

        var prevDueDate = task.DueDate;
        var prevLastCheckInDate = task.LastCheckInDate;

        var nextDue = TaskCycleHelpers.ComputeNextDueDate(task.DueDate, task.RecurrenceRule, req.ClientToday);
        task.Status = ByteTaskStatus.pending;
        task.DueDate = nextDue;
        task.CompletedAt = null;
        task.Submitted = false;
        task.LastCheckInDate = req.ClientToday;
        // Task mutations flushed via tracker; no separate UpdateAsync.

        // Absolute-target semantics: counterValue is the desired daily total, so clear prior "log" cycles for today to avoid double-counting.
        if (counterValue.HasValue)
        {
            await _cycleRepo.DeleteDailyLogsAsync(req.TaskId, req.ClientToday);
        }

        var newCycle = new TaskCheckInCycle
        {
            TaskId = req.TaskId,
            CheckInDate = req.ClientToday,
            CounterValue = counterValue,
            CreatedAt = DateTime.UtcNow,
            CycleType = "checkin",
            PointsAwarded = pointsToAward,
            PointTransactionId = transaction?.TransactionId,
            PreviousDueDate = prevDueDate,
            PreviousLastCheckInDate = prevLastCheckInDate,
            PreviousStreakCount = prevStreakCount,
            PreviousLongestCount = prevLongestCount,
            PreviousStreakLastActivity = prevStreakLastActivity,
            PreviousStreakIsActive = prevStreakIsActive,
            PreviousStreakBonusMultiplier = prevStreakBonusMultiplier,
        };
        _db.TaskCheckInCycles.Add(newCycle);

        if (task.IsRecurring)
            await _subtaskRepo.ResetCompletionByTaskIdAsync(req.TaskId);

        // Second flush: cycle insert + tracked task/user updates batched in one round trip.
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Check-in complete for task {TaskId}: {Base}×{Mult}={Awarded} pts, streak {Count}",
            req.TaskId, basePoints, bonusMultiplier, pointsToAward, newStreakCount);
        return HandlerResult<CheckInResponse>.Ok(new CheckInResponse
        {
            PointsAwarded = pointsToAward,
            BasePoints = basePoints,
            NewBalance = user.CurrentBalance,
            RecurringDailyTotal = newRecurringTotal,
            StreakCount = newStreakCount,
            LongestCount = newLongestCount,
            BonusMultiplier = bonusMultiplier,
            StreakReset = streakReset,
            NextDueDate = nextDue?.ToString("yyyy-MM-dd") ?? string.Empty,
            CycleId = newCycle.CycleId,
        });
    }
}
