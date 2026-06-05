using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Points;

public sealed record SubmitPointsHandlerRequest(Guid UserId, SubmitPointsRequest Request);

public sealed class SubmitPointsHandler : IRequestHandler<SubmitPointsHandlerRequest, SubmitPointsResponse>
{
    private readonly ITaskRepository _taskRepo;
    private readonly IPointTransactionRepository _txRepo;
    private readonly IUserRepository _userRepo;
    private readonly IStreakRepository _streakRepo;
    private readonly ILogger<SubmitPointsHandler> _logger;

    public SubmitPointsHandler(
        ITaskRepository taskRepo,
        IPointTransactionRepository txRepo,
        IUserRepository userRepo,
        IStreakRepository streakRepo,
        ILogger<SubmitPointsHandler> logger)
    {
        _taskRepo = taskRepo;
        _txRepo = txRepo;
        _userRepo = userRepo;
        _streakRepo = streakRepo;
        _logger = logger;
    }

    public async Task<HandlerResult<SubmitPointsResponse>> HandleAsync(SubmitPointsHandlerRequest req, CancellationToken ct = default)
    {
        var userId = req.UserId;
        var request = req.Request;
        _logger.LogInformation("User {UserId} submitting points for {Count} tasks", userId, request.TaskIds.Count);

        if (request.TaskIds == null || request.TaskIds.Count == 0)
            return HandlerResult<SubmitPointsResponse>.BadRequest("No task IDs provided.");

        var alreadyEarnedRegularToday = await _txRepo.GetDailyEarnedBySourceTypeAsync(userId, DateTime.UtcNow, SourceType.task);
        var alreadyEarnedRecurringToday = await _txRepo.GetDailyEarnedBySourceTypeAsync(userId, DateTime.UtcNow, SourceType.recurring_task);

        var remainingRegularCap = Models.PointCaps.RegularDaily - alreadyEarnedRegularToday;
        var remainingRecurringCap = Models.PointCaps.RecurringDaily - alreadyEarnedRecurringToday;

        var regularPointsAwarded = 0;
        var recurringPointsAwarded = 0;
        var transactions = new List<PointTransactionDto>();
        var errors = new List<string>();
        var results = new List<TaskSubmissionResult>();

        void AddFailure(string taskId, string err)
        {
            errors.Add(err);
            results.Add(new TaskSubmissionResult { TaskId = taskId, Awarded = 0, BasePoints = 0, BonusMultiplier = 1.0m, Error = err });
        }

        var categoryEarnedRegular = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var categoryEarnedRecurring = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        async Task<int> GetCategoryRemaining(string category, SourceType sourceType)
        {
            var (cache, cap) = sourceType == SourceType.recurring_task
                ? (categoryEarnedRecurring, Models.PointCaps.PerCategoryRecurringDaily)
                : (categoryEarnedRegular, Models.PointCaps.PerCategoryRegularDaily);
            if (!cache.TryGetValue(category, out var earned))
            {
                earned = await _txRepo.GetDailyEarnedByCategoryAsync(userId, DateTime.UtcNow, category, sourceType);
                cache[category] = earned;
            }
            return cap - earned;
        }

        foreach (var taskIdStr in request.TaskIds)
        {
            if (!Guid.TryParse(taskIdStr, out var taskId))
            {
                AddFailure(taskIdStr, $"Invalid task ID format: {taskIdStr}");
                continue;
            }

            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null || task.UserId != userId)
            {
                _logger.LogWarning("Task {TaskId} not found or does not belong to user {UserId}", taskId, userId);
                AddFailure(taskIdStr, $"Task {taskIdStr} was not found.");
                continue;
            }

            if (task.Status != ByteTaskStatus.completed)
            {
                _logger.LogWarning("Task {TaskId} is not completed — status is {Status}", taskId, task.Status);
                AddFailure(taskIdStr, $"Task '{task.Title}' is not completed yet.");
                continue;
            }

            int pointsToAward;
            int basePoints = task.PointValue;
            decimal bonusMultiplier = 1.0m;
            SourceType sourceType;

            if (task.IsRecurring)
            {
                var remainingForTask = remainingRecurringCap - recurringPointsAwarded;
                if (remainingForTask <= 0)
                {
                    AddFailure(taskIdStr, $"Recurring daily limit of {Models.PointCaps.RecurringDaily} reached — '{task.Title}' was not awarded points.");
                    continue;
                }
                var categoryRemaining = await GetCategoryRemaining(task.Category, SourceType.recurring_task);
                if (categoryRemaining <= 0)
                {
                    AddFailure(taskIdStr, $"Daily recurring {task.Category} cap of {Models.PointCaps.PerCategoryRecurringDaily} pts reached — '{task.Title}' was not awarded points.");
                    continue;
                }
                var streak = await _streakRepo.GetByTaskIdAsync(taskId);
                bonusMultiplier = streak?.BonusMultiplier ?? 1.0m;
                var multipliedPoints = (int)Math.Round(basePoints * (double)bonusMultiplier, MidpointRounding.AwayFromZero);
                pointsToAward = Math.Min(multipliedPoints, Math.Min(remainingForTask, categoryRemaining));
                sourceType = SourceType.recurring_task;
            }
            else
            {
                // Persisted Submitted flag is the source of truth, preventing duplicate submissions from double-awarding.
                if (task.Submitted == true)
                {
                    _logger.LogWarning("Points already submitted for task {TaskId}", taskId);
                    AddFailure(taskIdStr, $"Points for task '{task.Title}' have already been submitted.");
                    continue;
                }

                var remainingForTask = remainingRegularCap - regularPointsAwarded;
                if (remainingForTask <= 0)
                {
                    AddFailure(taskIdStr, $"Regular daily limit of {Models.PointCaps.RegularDaily} reached — '{task.Title}' was not awarded points.");
                    continue;
                }
                var categoryRemaining = await GetCategoryRemaining(task.Category, SourceType.task);
                if (categoryRemaining <= 0)
                {
                    AddFailure(taskIdStr, $"Daily {task.Category} cap of {Models.PointCaps.PerCategoryRegularDaily} pts reached — '{task.Title}' was not awarded points.");
                    continue;
                }
                pointsToAward = Math.Min(task.PointValue, Math.Min(remainingForTask, categoryRemaining));
                sourceType = SourceType.task;
            }

            var transaction = new PointTransaction
            {
                UserId = userId,
                Amount = pointsToAward,
                Type = TransactionType.EARN,
                SourceType = sourceType,
                Category = task.Category,
                Description = bonusMultiplier > 1.0m
                    ? $"Points earned for completing: {task.Title} ({basePoints} × {bonusMultiplier:0.0#}x streak)"
                    : $"Points earned for completing: {task.Title}",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _txRepo.CreateAsync(transaction);

            if (task.IsRecurring) recurringPointsAwarded += pointsToAward;
            else regularPointsAwarded += pointsToAward;

            var earnedCache = sourceType == SourceType.recurring_task ? categoryEarnedRecurring : categoryEarnedRegular;
            earnedCache[task.Category] = earnedCache[task.Category] + pointsToAward;

            task.Submitted = true;
            await _taskRepo.UpdateAsync(task);
            transactions.Add(new PointTransactionDto
            {
                TransactionId = created.TransactionId,
                UserId = created.UserId,
                Amount = created.Amount,
                Type = created.Type.ToString(),
                SourceType = created.SourceType?.ToString(),
                Category = created.Category,
                Description = created.Description,
                CreatedAt = created.CreatedAt
            });
            results.Add(new TaskSubmissionResult { TaskId = taskIdStr, Awarded = pointsToAward, BasePoints = basePoints, BonusMultiplier = bonusMultiplier, Error = null });

            _logger.LogInformation("Created transaction for task {TaskId} ({Type}) — {Points} points", taskId, sourceType, pointsToAward);
        }

        var totalPoints = regularPointsAwarded + recurringPointsAwarded;
        if (totalPoints > 0)
            await _userRepo.AddPointsAsync(userId, totalPoints);

        var newRegularTotal = alreadyEarnedRegularToday + regularPointsAwarded;
        var newRecurringTotal = alreadyEarnedRecurringToday + recurringPointsAwarded;
        var user = await _userRepo.GetByIdAsync(userId);

        _logger.LogInformation(
            "User {UserId} earned {Total} pts ({Regular} regular, {Recurring} recurring). Totals: {RegTotal}/{RegCap} regular, {RecTotal}/{RecCap} recurring",
            userId, totalPoints, regularPointsAwarded, recurringPointsAwarded,
            newRegularTotal, Models.PointCaps.RegularDaily, newRecurringTotal, Models.PointCaps.RecurringDaily);

        return HandlerResult<SubmitPointsResponse>.Ok(new SubmitPointsResponse
        {
            PointsAwarded = totalPoints,
            NewBalance = user?.CurrentBalance ?? 0,
            DailyTotal = newRegularTotal + newRecurringTotal,
            RecurringDailyTotal = newRecurringTotal,
            Transactions = transactions,
            Errors = errors,
            Results = results
        });
    }
}
