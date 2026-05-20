using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.BackgroundServices;

// Sweeps completed tasks older than retention window; runs every 24h on UTC date cutoff.
public class ArchiveJob : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ArchiveJob> _logger;

    public const int RetentionDays = 30;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(2);

    public ArchiveJob(IServiceProvider services, ILogger<ArchiveJob> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ArchiveJob starting; first run in {Delay}", StartupDelay);

        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ArchiveJob tick failed; will retry next interval");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (TaskCanceledException) { return; }
        }
    }

    private async Task RunOnceAsync()
    {
        using var scope = _services.CreateScope();
        var taskRepo = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

        var cutoff = DateTime.UtcNow.Date.AddDays(-RetentionDays);
        var archived = await taskRepo.AutoArchiveAsync(cutoff);

        _logger.LogInformation("ArchiveJob tick complete: {Archived} task(s) archived (cutoff {Cutoff:yyyy-MM-dd})", archived, cutoff);
    }
}
