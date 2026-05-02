using wahaha.API.Models.Domain;

namespace wahaha.API.Services.Interfaces;

public interface ITaskPenaltyService
{
    /// <summary>
    /// Threshold (in days past due) beyond which a non-recurring in_progress task is auto-demoted.
    /// </summary>
    int OverdueThresholdDays { get; }

    /// <summary>
    /// Returns true if the task qualifies for the auto-demotion penalty as of <paramref name="today"/>.
    /// </summary>
    bool ShouldPenalize(Models.Domain.Task task, DateTime today);

    /// <summary>
    /// Demotes any tasks in <paramref name="candidates"/> that qualify, sets WasPenalized=true,
    /// and persists the changes. Returns the number of tasks demoted.
    /// Mutates the entities in place so callers can return them in the same response.
    /// </summary>
    Task<int> ApplyAndPersistAsync(IEnumerable<Models.Domain.Task> candidates, DateTime today);
}
