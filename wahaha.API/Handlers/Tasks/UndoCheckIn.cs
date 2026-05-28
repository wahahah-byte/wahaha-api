using Microsoft.EntityFrameworkCore;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record UndoCheckInHandlerRequest(Guid TaskId, int CycleId, Guid UserId, DateTime ClientToday);

public sealed class UndoCheckInHandler : IRequestHandler<UndoCheckInHandlerRequest, UndoCheckInResponse>
{
    private readonly ITaskRepository _taskRepo;
    private readonly IPointTransactionRepository _txRepo;
    private readonly IUserRepository _userRepo;
    private readonly IStreakRepository _streakRepo;
    private readonly ITaskCheckInCycleRepository _cycleRepo;
    private readonly ILogger<UndoCheckInHandler> _logger;

    public UndoCheckInHandler(
        ITaskRepository taskRepo,
        IPointTransactionRepository txRepo,
        IUserRepository userRepo,
        IStreakRepository streakRepo,
        ITaskCheckInCycleRepository cycleRepo,
        ILogger<UndoCheckInHandler> logger)
    {
        _taskRepo = taskRepo;
        _txRepo = txRepo;
        _userRepo = userRepo;
        _streakRepo = streakRepo;
        _cycleRepo = cycleRepo;
        _logger = logger;
    }

    public async Task<HandlerResult<UndoCheckInResponse>> HandleAsync(UndoCheckInHandlerRequest req, CancellationToken ct = default)
    {
        try
        {
            return await HandleCore(req, ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Undo on task {TaskId} cycle {CycleId} lost a concurrency race; treating as success", req.TaskId, req.CycleId);
            var refreshedUser = await _userRepo.GetByIdAsync(req.UserId);
            var refreshedTotal = await _txRepo.GetDailyEarnedBySourceTypeAsync(req.UserId, DateTime.UtcNow, SourceType.recurring_task);
            return HandlerResult<UndoCheckInResponse>.Ok(new UndoCheckInResponse
            {
                NewBalance = refreshedUser?.CurrentBalance ?? 0,
                RecurringDailyTotal = refreshedTotal,
                StreakCount = 0,
                LongestCount = 0,
                BonusMultiplier = 1.0m,
                PreviousDueDate = string.Empty,
                PreviousLastCheckInDate = string.Empty,
                PointsRefunded = 0,
            });
        }
    }

    private async Task<HandlerResult<UndoCheckInResponse>> HandleCore(UndoCheckInHandlerRequest req, CancellationToken ct)
    {
        _logger.LogInformation("User {UserId} undoing check-in cycle {CycleId} on task {TaskId}", req.UserId, req.CycleId, req.TaskId);

        var task = await _taskRepo.GetByIdAsync(req.TaskId);
        if (task == null || task.UserId != req.UserId)
            return HandlerResult<UndoCheckInResponse>.NotFound($"Task with ID {req.TaskId} was not found.");

        var cycle = await _cycleRepo.GetByIdAsync(req.CycleId);
        if (cycle == null || cycle.TaskId != req.TaskId)
            return HandlerResult<UndoCheckInResponse>.NotFound($"Check-in cycle {req.CycleId} was not found for task {req.TaskId}.");

        if (cycle.CycleType != "checkin")
            return HandlerResult<UndoCheckInResponse>.BadRequest("This cycle is not a check-in and cannot be undone via this endpoint.");

        var latestCheckin = await _cycleRepo.GetLatestCheckinByTaskIdAsync(req.TaskId);
        if (latestCheckin == null)
            return HandlerResult<UndoCheckInResponse>.BadRequest("No check-in cycle to undo.");
        
        if (latestCheckin.CycleId != cycle.CycleId)
        {
            cycle = latestCheckin;
        }

        if (cycle.CheckInDate.Date != req.ClientToday.Date)
        {
  
            if (task.LastCheckInDate?.Date == req.ClientToday.Date)
            {
                _logger.LogWarning(
                    "Task {TaskId} in inconsistent state: LastCheckInDate={LastCheckInDate} is today but latest cycle {CycleId} is dated {CycleDate}. Repairing task state.",
                    req.TaskId, task.LastCheckInDate, cycle.CycleId, cycle.CheckInDate);
                await _taskRepo.SetCycleStateAsync(req.TaskId, cycle.PreviousDueDate, cycle.PreviousLastCheckInDate);
                var repairedUser = await _userRepo.GetByIdAsync(req.UserId);
                var repairedTotal = await _txRepo.GetDailyEarnedBySourceTypeAsync(req.UserId, DateTime.UtcNow, SourceType.recurring_task);
                return HandlerResult<UndoCheckInResponse>.Ok(new UndoCheckInResponse
                {
                    NewBalance = repairedUser?.CurrentBalance ?? 0,
                    RecurringDailyTotal = repairedTotal,
                    StreakCount = 0,
                    LongestCount = 0,
                    BonusMultiplier = 1.0m,
                    PreviousDueDate = cycle.PreviousDueDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    PreviousLastCheckInDate = cycle.PreviousLastCheckInDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    PointsRefunded = 0,
                });
            }
            return HandlerResult<UndoCheckInResponse>.BadRequest("Check-ins can only be undone on the same day they were made.");
        }

        var pointsRefunded = cycle.PointsAwarded ?? 0;
        if (cycle.PointTransactionId.HasValue)
            await _txRepo.DeleteAsync(cycle.PointTransactionId.Value);
        if (pointsRefunded > 0)
            await _userRepo.RefundPointsAsync(req.UserId, pointsRefunded);

        var streak = await _streakRepo.GetByTaskIdAsync(req.TaskId);
        var restoredCount = cycle.PreviousStreakCount ?? 0;
        var restoredLongest = cycle.PreviousLongestCount ?? streak?.LongestCount ?? 0;
        var restoredLastActivity = cycle.PreviousStreakLastActivity ?? streak?.LastActivityDate ?? DateTime.UtcNow;
        var restoredIsActive = cycle.PreviousStreakIsActive ?? streak?.IsActive ?? true;
        var restoredMultiplier = cycle.PreviousStreakBonusMultiplier ?? streak?.BonusMultiplier ?? 1.0m;
        if (streak != null)
        {
            await _streakRepo.RestoreAsync(
                streak.StreakId,
                restoredCount,
                restoredLongest,
                restoredLastActivity,
                restoredIsActive,
                restoredMultiplier);
        }

        await _taskRepo.SetCycleStateAsync(req.TaskId, cycle.PreviousDueDate, cycle.PreviousLastCheckInDate);

        // Preserve any counter value that was committed into this check-in cycle
        // (e.g. the buffered tap-log delta absorbed at slide-commit time, or the
        // amount entered through the custom-log modal's overshoot path). Without
        // this, the value disappears with the deleted cycle and the logger
        // displays zero after undo while the user expects to see what they had
        // logged. Re-creating it as a "log" cycle restores the daily-counter sum
        // computed client-side from recentCycles. counterValue is stored as 0 in
        // some legacy rows, so guard with > 0 to avoid junk log entries.
        var restoredCounterValue = cycle.CounterValue;
        var restoredCheckInDate = cycle.CheckInDate;

        await _cycleRepo.DeleteAsync(cycle.CycleId);

        if (restoredCounterValue.HasValue && restoredCounterValue.Value > 0)
        {
            await _cycleRepo.CreateAsync(new TaskCheckInCycle
            {
                TaskId = req.TaskId,
                CheckInDate = restoredCheckInDate,
                CounterValue = restoredCounterValue,
                CreatedAt = DateTime.UtcNow,
                CycleType = "log",
            });
        }

        var user = await _userRepo.GetByIdAsync(req.UserId);
        var newRecurringTotal = await _txRepo.GetDailyEarnedBySourceTypeAsync(req.UserId, DateTime.UtcNow, SourceType.recurring_task);

        _logger.LogInformation("Check-in cycle {CycleId} undone: refunded {Points} pts, streak restored to {Count}",
            req.CycleId, pointsRefunded, restoredCount);

        return HandlerResult<UndoCheckInResponse>.Ok(new UndoCheckInResponse
        {
            NewBalance = user?.CurrentBalance ?? 0,
            RecurringDailyTotal = newRecurringTotal,
            StreakCount = restoredCount,
            LongestCount = restoredLongest,
            BonusMultiplier = restoredMultiplier,
            PreviousDueDate = cycle.PreviousDueDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            PreviousLastCheckInDate = cycle.PreviousLastCheckInDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            PointsRefunded = pointsRefunded,
        });
    }
}
