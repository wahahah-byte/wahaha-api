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
        // Defensive catch — the second SaveChangesAsync near the bottom uses
        // the change tracker for task + user + (sometimes) streak updates,
        // and a concurrent undo flow touching the same rows can still race
        // here even though the individual atomic ops above can't. Treating
        // a lost race as "Already checked in" matches the user's intent: a
        // concurrent undo just rolled back state, and the client's optimistic
        // patch + suppress-already-checked-in handling will reconcile.
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

        // Idempotency. A successful check-in advances DueDate by one period
        // (daily: tomorrow, weekly: next Friday, etc.), so the user shouldn't
        // be able to commit another check-in until today catches up to that
        // DueDate. The previous check (`LastCheckInDate > cycleStart`) was
        // incorrect: for a daily task just-checked-in, cycleStart computed
        // from DueDate=tomorrow is today, and `LastCheckInDate (today) >
        // today` is false — so duplicate same-day check-ins slipped through
        // and each one bumped DueDate by an extra day. Comparing today
        // against DueDate directly catches that and still allows the legit
        // "next day" check-in (today >= DueDate).
        if (task.LastCheckInDate.HasValue && task.DueDate.HasValue)
        {
            if (req.ClientToday.Date < task.DueDate.Value.Date)
                return HandlerResult<CheckInResponse>.BadRequest("Already checked in for this cycle.");
        }

        // Load the user up front instead of after every write — saves a
        // trailing GetByIdAsync round trip. Modifications below are made
        // on the tracked entity and flushed in the single SaveChangesAsync
        // at the end of the happy path.
        var user = await _userRepo.GetByIdAsync(req.UserId);
        if (user == null)
            return HandlerResult<CheckInResponse>.NotFound("User not found.");

        var streakType = $"{task.RecurrenceRule}_{task.Category}";
        var streak = await _streakRepo.GetByTaskIdAsync(req.TaskId);
        var streakReset = false;
        if (streak == null)
        {
            // Stage the new streak in the tracker (not via CreateAsync which
            // would SaveChanges immediately). It'll be inserted in the
            // batched SaveChanges below.
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
            // Streak resets when the task is overdue at the moment of check-in
            // (i.e. the user missed the previous cycle's window). Using the
            // task's DueDate as the trigger keeps the rule consistent with the
            // user's mental model — "overdue → reset" — across all recurrence
            // rules, including weekdays where the Fri→Mon weekend gap is
            // naturally handled by DueDate jumping to Monday (so a Monday
            // check-in is on-time and doesn't reset).
            //
            // Previously this used (today - streak.LastActivityDate) > maxGapDays
            // with a per-rule tolerance. That tolerated 1-2 day gaps for
            // weekdays tasks, which diverged from "overdue = reset" and made
            // the client's optimistic streak prediction bounce on the wire.
            var isOverdue = task.DueDate.HasValue && req.ClientToday.Date > task.DueDate.Value.Date;
            if (isOverdue)
            {
                // Stage the reset on the tracked entity. SaveChanges below
                // will flush these property changes alongside everything else.
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

        // Compute the post-increment streak state in-memory — avoids the
        // post-IncrementAsync GetByIdAsync that used to re-fetch the row.
        // The values mirror what IncrementAsync's SQL CASE expression
        // computes; the shared helper keeps the C# and SQL in sync.
        var newStreakCount = prevStreakCount + 1;
        var newLongestCount = Math.Max(prevLongestCount, newStreakCount);
        var bonusMultiplier = StreakBonusMultiplier.Compute(newStreakCount);

        // Three streak-update paths:
        //   1. Added — brand new streak instance staged above. Set values
        //      directly; SaveChanges below inserts the row.
        //   2. Reset path — existing streak had a too-large gap, we reset
        //      CurrentCount to 0 in-memory above. Setting values directly
        //      here means SaveChanges flushes BOTH the reset and the
        //      increment in one UPDATE. Using IncrementAsync here would
        //      add 1 to the DB's pre-reset value (5+1=6), since
        //      ExecuteUpdate doesn't see the in-memory reset and then
        //      detaches the entity so the reset is lost on SaveChanges.
        //   3. Default — existing streak, no reset. Use the atomic
        //      ExecuteUpdate path for concurrency safety against parallel
        //      rapid-tap requests.
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

        // Combined daily-totals query — was previously two separate SUMs.
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

        // Stage the point transaction so we can flush it ahead of the cycle
        // row (the cycle has a PointTransactionId FK and there's no
        // navigation property linking the two, so EF can't auto-wire the
        // generated TransactionId after batched insert — we need it in hand
        // before the cycle row is built). One extra SaveChanges round trip
        // for the dependency; everything else (cycle insert + task update +
        // user update + new/reset streak) batches together at the end.
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
            // Flush tx so we can read back the generated TransactionId. Any
            // streak insert/update tracked above is flushed alongside in
            // the same batch — no separate round trip for it.
            await _db.SaveChangesAsync(ct);

            // Mutate user balance in-memory; SaveChanges at the end picks up
            // the change. Skips a separate AddPointsAsync round trip.
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
        // Mark task as Modified via the tracker (no separate UpdateAsync /
        // SaveChanges round trip).

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

        // Second flush: cycle insert + task update (via tracker) + user
        // balance update (via tracker) all batched into one round trip.
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
