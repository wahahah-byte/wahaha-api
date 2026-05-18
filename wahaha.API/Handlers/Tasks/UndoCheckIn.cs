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
        _logger.LogInformation("User {UserId} undoing check-in cycle {CycleId} on task {TaskId}", req.UserId, req.CycleId, req.TaskId);

        var task = await _taskRepo.GetByIdAsync(req.TaskId);
        if (task == null || task.UserId != req.UserId)
            return HandlerResult<UndoCheckInResponse>.NotFound($"Task with ID {req.TaskId} was not found.");

        var cycle = await _cycleRepo.GetByIdAsync(req.CycleId);
        if (cycle == null || cycle.TaskId != req.TaskId)
            return HandlerResult<UndoCheckInResponse>.NotFound($"Check-in cycle {req.CycleId} was not found for task {req.TaskId}.");

        if (cycle.CycleType != "checkin")
            return HandlerResult<UndoCheckInResponse>.BadRequest("This cycle is not a check-in and cannot be undone via this endpoint.");

        // Compare against the latest CHECKIN cycle, not the latest cycle of
        // any type — a counter quick-log ("log" cycleType) for the same day
        // can land after the check-in and would otherwise lock the check-in
        // out of being undone. Logs are independent records of progress
        // toward the goal and don't need to be reversed when the check-in
        // commitment is undone; they stay as historical counter values.
        var latestCheckin = await _cycleRepo.GetLatestCheckinByTaskIdAsync(req.TaskId);
        if (latestCheckin == null || latestCheckin.CycleId != cycle.CycleId)
            return HandlerResult<UndoCheckInResponse>.BadRequest("Only the most recent check-in can be undone.");

        if (cycle.CheckInDate.Date != req.ClientToday.Date)
            return HandlerResult<UndoCheckInResponse>.BadRequest("Check-ins can only be undone on the same day they were made.");

        var pointsRefunded = cycle.PointsAwarded ?? 0;
        if (cycle.PointTransactionId.HasValue)
            await _txRepo.DeleteAsync(cycle.PointTransactionId.Value);
        if (pointsRefunded > 0)
            await _userRepo.RefundPointsAsync(req.UserId, pointsRefunded);

        var streak = await _streakRepo.GetByTaskIdAsync(req.TaskId);
        if (streak != null)
        {
            streak.CurrentCount = cycle.PreviousStreakCount ?? 0;
            streak.LongestCount = cycle.PreviousLongestCount ?? streak.LongestCount;
            streak.LastActivityDate = cycle.PreviousStreakLastActivity ?? streak.LastActivityDate;
            streak.IsActive = cycle.PreviousStreakIsActive ?? streak.IsActive;
            streak.BonusMultiplier = cycle.PreviousStreakBonusMultiplier ?? streak.BonusMultiplier;
            await _streakRepo.UpdateAsync(streak);
        }

        task.DueDate = cycle.PreviousDueDate;
        task.LastCheckInDate = cycle.PreviousLastCheckInDate;
        await _taskRepo.UpdateAsync(task);

        await _cycleRepo.DeleteAsync(cycle.CycleId);

        var user = await _userRepo.GetByIdAsync(req.UserId);
        var newRecurringTotal = await _txRepo.GetDailyEarnedBySourceTypeAsync(req.UserId, DateTime.UtcNow, SourceType.recurring_task);

        _logger.LogInformation("Check-in cycle {CycleId} undone: refunded {Points} pts, streak restored to {Count}",
            req.CycleId, pointsRefunded, streak?.CurrentCount);

        return HandlerResult<UndoCheckInResponse>.Ok(new UndoCheckInResponse
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
}
