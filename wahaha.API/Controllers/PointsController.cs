using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PointsController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly IPointTransactionRepository _pointTransactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PointsController> _logger;

    public PointsController(
        ITaskRepository taskRepository,
        IPointTransactionRepository pointTransactionRepository,
        IUserRepository userRepository,
        ILogger<PointsController> logger)
    {
        _taskRepository = taskRepository;
        _pointTransactionRepository = pointTransactionRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpPost("submit")]
    public async Task<ActionResult<SubmitPointsResponse>> Submit([FromBody] SubmitPointsRequest request)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} submitting points for {Count} tasks", userId, request.TaskIds.Count);

        if (request.TaskIds == null || request.TaskIds.Count == 0)
            return BadRequest("No task IDs provided.");

        var alreadyEarnedRegularToday = await _pointTransactionRepository.GetDailyEarnedBySourceTypeAsync(userId, DateTime.UtcNow, SourceType.task);
        var alreadyEarnedRecurringToday = await _pointTransactionRepository.GetDailyEarnedBySourceTypeAsync(userId, DateTime.UtcNow, SourceType.recurring_task);

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
            results.Add(new TaskSubmissionResult { TaskId = taskId, Awarded = 0, Error = err });
        }

        // Per-category daily earned, split by source type, fetched lazily and tracked through this submission
        var categoryEarnedRegular = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var categoryEarnedRecurring = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        async Task<int> GetCategoryRemaining(string category, SourceType sourceType)
        {
            var (cache, cap) = sourceType == SourceType.recurring_task
                ? (categoryEarnedRecurring, Models.PointCaps.PerCategoryRecurringDaily)
                : (categoryEarnedRegular, Models.PointCaps.PerCategoryRegularDaily);
            if (!cache.TryGetValue(category, out var earned))
            {
                earned = await _pointTransactionRepository.GetDailyEarnedByCategoryAsync(userId, DateTime.UtcNow, category, sourceType);
                cache[category] = earned;
            }
            return cap - earned;
        }

        // Fetch existing transactions once for duplicate checks on regular tasks
        var existingTransactions = await _pointTransactionRepository.GetByUserAsync(userId);

        foreach (var taskIdStr in request.TaskIds)
        {
            if (!Guid.TryParse(taskIdStr, out var taskId))
            {
                AddFailure(taskIdStr, $"Invalid task ID format: {taskIdStr}");
                continue;
            }

            var task = await _taskRepository.GetByIdAsync(taskId);

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
                pointsToAward = Math.Min(task.PointValue, Math.Min(remainingForTask, categoryRemaining));
                sourceType = SourceType.recurring_task;
            }
            else
            {
                var alreadySubmitted = existingTransactions.Any(t =>
                    t.SourceType == SourceType.task &&
                    t.SourceId == task.TaskId.GetHashCode());

                if (alreadySubmitted)
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
                Description = $"Points earned for completing: {task.Title}",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _pointTransactionRepository.CreateAsync(transaction);

            if (task.IsRecurring)
                recurringPointsAwarded += pointsToAward;
            else
                regularPointsAwarded += pointsToAward;

            var earnedCache = sourceType == SourceType.recurring_task ? categoryEarnedRecurring : categoryEarnedRegular;
            earnedCache[task.Category] = earnedCache[task.Category] + pointsToAward;

            task.Submitted = true;
            await _taskRepository.UpdateAsync(task);
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
            results.Add(new TaskSubmissionResult { TaskId = taskIdStr, Awarded = pointsToAward, Error = null });

            _logger.LogInformation("Created transaction for task {TaskId} ({Type}) — {Points} points", taskId, sourceType, pointsToAward);
        }

        var totalPoints = regularPointsAwarded + recurringPointsAwarded;

        if (totalPoints > 0)
            await _userRepository.AddPointsAsync(userId, totalPoints);

        var newRegularTotal = alreadyEarnedRegularToday + regularPointsAwarded;
        var newRecurringTotal = alreadyEarnedRecurringToday + recurringPointsAwarded;
        var user = await _userRepository.GetByIdAsync(userId);

        _logger.LogInformation(
            "User {UserId} earned {Total} pts ({Regular} regular, {Recurring} recurring). Totals: {RegTotal}/{RegCap} regular, {RecTotal}/{RecCap} recurring",
            userId, totalPoints, regularPointsAwarded, recurringPointsAwarded,
            newRegularTotal, Models.PointCaps.RegularDaily, newRecurringTotal, Models.PointCaps.RecurringDaily);

        return Ok(new SubmitPointsResponse
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
