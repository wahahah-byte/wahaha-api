using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Services;

public class TaskPenaltyService : ITaskPenaltyService
{
    private readonly WahahaDbContext _context;
    private readonly ILogger<TaskPenaltyService> _logger;

    public int OverdueThresholdDays => 3;

    public TaskPenaltyService(WahahaDbContext context, ILogger<TaskPenaltyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public bool ShouldPenalize(Models.Domain.Task task, DateTime today)
    {
        if (task.Status != ByteTaskStatus.in_progress) return false;
        if (task.IsRecurring) return false;
        if (!task.DueDate.HasValue) return false;
        var daysOverdue = (today.Date - task.DueDate.Value.Date).Days;
        return daysOverdue >= OverdueThresholdDays;
    }

    public async Task<int> ApplyAndPersistAsync(IEnumerable<Models.Domain.Task> candidates, DateTime today)
    {
        var demoted = 0;
        foreach (var task in candidates)
        {
            if (!ShouldPenalize(task, today)) continue;
            task.Status = ByteTaskStatus.pending;
            task.WasPenalized = true;
            demoted++;
        }
        if (demoted > 0)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Penalty applied to {Count} task(s) (cutoff date {Today:yyyy-MM-dd})", demoted, today);
        }
        return demoted;
    }
}
