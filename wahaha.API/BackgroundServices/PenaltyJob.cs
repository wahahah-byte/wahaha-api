using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.BackgroundServices;

/// <summary>
/// Sweeps overdue non-recurring in_progress tasks and applies the auto-demotion penalty.
/// Runs once at startup, then every 24 hours. Uses UTC date as the threshold (no client
/// timezone available in a server-side job); the on-read path in TasksController.GetAll
/// catches any timezone-edge cases the moment the user opens the app.
/// </summary>
public class PenaltyJob : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<PenaltyJob> _logger;

    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(1);

    public PenaltyJob(IServiceProvider services, ILogger<PenaltyJob> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PenaltyJob starting; first run in {Delay}", StartupDelay);

        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PenaltyJob tick failed; will retry next interval");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (TaskCanceledException) { return; }
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var taskRepo = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
        var penaltyService = scope.ServiceProvider.GetRequiredService<ITaskPenaltyService>();

        var today = DateTime.UtcNow.Date;
        var cutoff = today.AddDays(-(penaltyService.OverdueThresholdDays - 1));

        var candidates = await taskRepo.GetPenaltyCandidatesAsync(cutoff);
        var demoted = await penaltyService.ApplyAndPersistAsync(candidates, today);

        _logger.LogInformation("PenaltyJob tick complete: {Demoted} task(s) demoted (UTC date {Today:yyyy-MM-dd})", demoted, today);
    }
}
