using wahaha.API.Models.Domain;

namespace wahaha.API.Services.Interfaces;

public interface ITaskPenaltyService
{
    // Days-past-due threshold for auto-demoting non-recurring in_progress tasks.
    int OverdueThresholdDays { get; }

    // True when task qualifies for the auto-demotion penalty as of today.
    bool ShouldPenalize(Models.Domain.Task task, DateTime today);

    // Demote qualifying candidates, set WasPenalized, persist; returns count demoted.
    Task<int> ApplyAndPersistAsync(IEnumerable<Models.Domain.Task> candidates, DateTime today);
}
