using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record CheckInTaskRequest(Guid TaskId, Guid UserId, DateTime ClientToday, CheckInRequest? Request);

public sealed class CheckInTaskHandler : IRequestHandler<CheckInTaskRequest, CheckInResponse>
{
    private readonly ITaskRepository _taskRepo;
    private readonly IPointTransactionRepository _txRepo;
    private readonly IUserRepository _userRepo;
    private readonly IStreakRepository _streakRepo;
    private readonly ITaskCheckInCycleRepository _cycleRepo;
    private readonly ISubtaskRepository _subtaskRepo;
    private readonly ILogger<CheckInTaskHandler> _logger;

    public CheckInTaskHandler(
        ITaskRepository taskRepo,
        IPointTransactionRepository txRepo,
        IUserRepository userRepo,
        IStreakRepository streakRepo,
        ITaskCheckInCycleRepository cycleRepo,
        ISubtaskRepository subtaskRepo,
        ILogger<CheckInTaskHandler> logger)
    {
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
        }

        // Idempotency.
        if (task.LastCheckInDate.HasValue && task.DueDate.HasValue)
        {
            var cycleStart = TaskCycleHelpers.GetCycleStart(task.DueDate.Value.Date, task.RecurrenceRule);
            if (task.LastCheckInDate.Value.Date > cycleStart)
                return HandlerResult<CheckInResponse>.BadRequest("Already checked in for this cycle.");
        }

        var streakType = $"{task.RecurrenceRule}_{task.Category}";
        var streak = await _streakRepo.GetByTaskIdAsync(req.TaskId);
        var streakReset = false;
        if (streak == null)
        {
            streak = await _streakRepo.CreateAsync(new Streak
            {
                UserId = req.UserId,
                TaskId = req.TaskId,
                StreakType = streakType,
                CurrentCount = 0,
                LongestCount = 0,
                BonusMultiplier = 1.0m,
                IsActive = true,
                LastActivityDate = DateTime.UtcNow,
            });
        }
        else
        {
            var maxGapDays = task.RecurrenceRule switch
            {
                "daily"    => 1,
                "weekdays" => 3,
                "weekly"   => 7,
                "biweekly" => 14,
                "monthly"  => 31,
                _          => 1,
            };
            var daysSinceLast = (req.ClientToday.Date - streak.LastActivityDate.Date).Days;
            if (daysSinceLast > maxGapDays)
            {
                await _streakRepo.ResetAsync(streak.StreakId);
                streakReset = true;
            }
        }

        var prevStreakCount = streak.CurrentCount;
        var prevLongestCount = streak.LongestCount;
        var prevStreakLastActivity = streak.LastActivityDate;
        var prevStreakIsActive = streak.IsActive;
        var prevStreakBonusMultiplier = streak.BonusMultiplier;

        await _streakRepo.IncrementAsync(streak.StreakId, req.ClientToday);
        var updatedStreak = await _streakRepo.GetByIdAsync(streak.StreakId);
        var bonusMultiplier = updatedStreak?.BonusMultiplier ?? 1.0m;

        var alreadyEarned = await _txRepo.GetDailyEarnedBySourceTypeAsync(req.UserId, DateTime.UtcNow, SourceType.recurring_task);
        var alreadyEarnedInCategory = await _txRepo.GetDailyEarnedByCategoryAsync(req.UserId, DateTime.UtcNow, task.Category, SourceType.recurring_task);

        if (alreadyEarned >= Models.PointCaps.RecurringDaily)
            return HandlerResult<CheckInResponse>.BadRequest("Daily check-in limit reached.");
        var categoryRemaining = Models.PointCaps.PerCategoryRecurringDaily - alreadyEarnedInCategory;
        if (categoryRemaining <= 0)
            return HandlerResult<CheckInResponse>.BadRequest($"Daily recurring {task.Category} cap of {Models.PointCaps.PerCategoryRecurringDaily} pts reached.");
        var sourceRemaining = Models.PointCaps.RecurringDaily - alreadyEarned;
        var basePoints = task.PointValue;
        var multipliedPoints = (int)Math.Round(basePoints * (double)bonusMultiplier, MidpointRounding.AwayFromZero);
        var pointsToAward = Math.Max(0, Math.Min(multipliedPoints, Math.Min(sourceRemaining, categoryRemaining)));

        int? pointTransactionId = null;
        if (pointsToAward > 0)
        {
            var transaction = new PointTransaction
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
            await _txRepo.CreateAsync(transaction);
            pointTransactionId = transaction.TransactionId;
            await _userRepo.AddPointsAsync(req.UserId, pointsToAward);
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
        await _taskRepo.UpdateAsync(task);

        var newCycle = await _cycleRepo.CreateAsync(new TaskCheckInCycle
        {
            TaskId = req.TaskId,
            CheckInDate = req.ClientToday,
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

        if (task.IsRecurring)
            await _subtaskRepo.ResetCompletionByTaskIdAsync(req.TaskId);

        var user = await _userRepo.GetByIdAsync(req.UserId);

        _logger.LogInformation("Check-in complete for task {TaskId}: {Base}×{Mult}={Awarded} pts, streak {Count}",
            req.TaskId, basePoints, bonusMultiplier, pointsToAward, updatedStreak?.CurrentCount);
        return HandlerResult<CheckInResponse>.Ok(new CheckInResponse
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
}
