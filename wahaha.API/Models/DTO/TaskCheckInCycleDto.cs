namespace wahaha.API.Models.DTO;

public class CheckInCycleDto
{
    public int CycleId { get; set; }
    public Guid TaskId { get; set; }
    public DateTime CheckInDate { get; set; }
    public int? CounterValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CycleType { get; set; } = "checkin";
    // Snapshot of task.DueDate from BEFORE this check-in advanced it. Lets
    // the client restore the precise pre-checkin overdue state on undo for
    // cycles loaded from a previous session (preCheckinDueDateRef only
    // covers same-session check-ins).
    public DateTime? PreviousDueDate { get; set; }
}

public class CheckInRequest
{
    public int? CounterValue { get; set; }
}

public class UndoCheckInResponse
{
    public int NewBalance { get; set; }
    public int RecurringDailyTotal { get; set; }
    public int StreakCount { get; set; }
    public int LongestCount { get; set; }
    public decimal BonusMultiplier { get; set; }
    public string PreviousDueDate { get; set; } = string.Empty;
    // Empty string means "no prior check-in" (null in DB) — the client treats
    // it as null so the task counts as never-checked-in and unlocks immediately.
    public string PreviousLastCheckInDate { get; set; } = string.Empty;
    public int PointsRefunded { get; set; }
}
