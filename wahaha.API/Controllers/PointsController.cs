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

    private const int DailyPointCap = 200;

    [HttpPost("submit")]
    public async Task<ActionResult<SubmitPointsResponse>> Submit([FromBody] SubmitPointsRequest request)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} submitting points for {Count} tasks", userId, request.TaskIds.Count);

        if (request.TaskIds == null || request.TaskIds.Count == 0)
            return BadRequest("No task IDs provided.");

        // Check how many points the user has already earned today
        var alreadyEarnedToday = await _pointTransactionRepository.GetDailyEarnedAsync(userId, DateTime.UtcNow);
        var remainingCap = DailyPointCap - alreadyEarnedToday;

        if (remainingCap <= 0)
        {
            _logger.LogInformation("User {UserId} has reached the daily point cap", userId);
            return BadRequest($"Daily point limit of {DailyPointCap} already reached.");
        }

        var totalPoints = 0;
        var transactions = new List<PointTransactionDto>();
        var errors = new List<string>();

        // Fetch existing transactions once for duplicate checks
        var existingTransactions = await _pointTransactionRepository.GetByUserAsync(userId);

        foreach (var taskIdStr in request.TaskIds)
        {
            if (remainingCap - totalPoints <= 0)
            {
                errors.Add($"Daily limit reached — remaining tasks were not awarded points.");
                break;
            }

            if (!Guid.TryParse(taskIdStr, out var taskId))
            {
                errors.Add($"Invalid task ID format: {taskIdStr}");
                continue;
            }

            var task = await _taskRepository.GetByIdAsync(taskId);

            if (task == null || task.UserId != userId)
            {
                _logger.LogWarning("Task {TaskId} not found or does not belong to user {UserId}", taskId, userId);
                errors.Add($"Task {taskIdStr} was not found.");
                continue;
            }

            if (task.Status != ByteTaskStatus.completed)
            {
                _logger.LogWarning("Task {TaskId} is not completed — status is {Status}", taskId, task.Status);
                errors.Add($"Task '{task.Title}' is not completed yet.");
                continue;
            }

            var alreadySubmitted = existingTransactions.Any(t =>
                t.SourceType == SourceType.task &&
                t.SourceId == task.TaskId.GetHashCode());

            if (alreadySubmitted)
            {
                _logger.LogWarning("Points already submitted for task {TaskId}", taskId);
                errors.Add($"Points for task '{task.Title}' have already been submitted.");
                continue;
            }

            // Cap the points awarded for this task if it would exceed the daily limit
            var pointsToAward = Math.Min(task.PointValue, remainingCap - totalPoints);

            var transaction = new PointTransaction
            {
                UserId = userId,
                Amount = pointsToAward,
                Type = TransactionType.EARN,
                SourceType = SourceType.task,
                Description = $"Points earned for completing: {task.Title}",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _pointTransactionRepository.CreateAsync(transaction);
            totalPoints += pointsToAward;
            task.Submitted = true;
            await _taskRepository.UpdateAsync(task);
            transactions.Add(new PointTransactionDto
            {
                TransactionId = created.TransactionId,
                UserId = created.UserId,
                Amount = created.Amount,
                Type = created.Type.ToString(),
                SourceType = created.SourceType?.ToString(),
                Description = created.Description,
                CreatedAt = created.CreatedAt
            });

            _logger.LogInformation("Created transaction for task {TaskId} — {Points} points", taskId, pointsToAward);
        }

        // Update user balance
        if (totalPoints > 0)
            await _userRepository.AddPointsAsync(userId, totalPoints);

        var newDailyTotal = alreadyEarnedToday + totalPoints;
        var user = await _userRepository.GetByIdAsync(userId);

        _logger.LogInformation("User {UserId} earned {TotalPoints} points from {Count} tasks (daily total: {DailyTotal}/{Cap})",
            userId, totalPoints, transactions.Count, newDailyTotal, DailyPointCap);

        return Ok(new SubmitPointsResponse
        {
            PointsAwarded = totalPoints,
            NewBalance = user?.CurrentBalance ?? 0,
            DailyTotal = newDailyTotal,
            Transactions = transactions,
            Errors = errors
        });
    }
}
