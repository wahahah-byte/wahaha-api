namespace wahaha.API.Models.DTO;

public class TaskDto
{
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int PointValue { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; }
    public bool Submitted { get; set; } = false;
    public bool WasPenalized { get; set; } = false;
    public int? CurrentStreakCount { get; set; }
    public int? LongestStreakCount { get; set; }
    public DateTime? LastCheckInDate { get; set; }
    public bool IsArchived { get; set; } = false;
    public bool HasCounter { get; set; } = false;
    public string? CounterUnit { get; set; }
    public int? CounterGoal { get; set; }
    public List<SubtaskDto> Subtasks { get; set; } = new();
    public List<CheckInCycleDto> RecentCycles { get; set; } = new();
}

public class CreateTaskDto
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public int PointValue { get; set; } = 10;
    public DateTime? DueDate { get; set; }
    public bool IsRecurring { get; set; } = false;
    public string? RecurrenceRule { get; set; }
    public bool Submitted { get; set; } = false;
    public bool HasCounter { get; set; } = false;
    public string? CounterUnit { get; set; }
    public int? CounterGoal { get; set; }
}

public class UpdateTaskDto
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int PointValue { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; }
    public bool? Submitted { get; set; }
    public bool HasCounter { get; set; } = false;
    public string? CounterUnit { get; set; }
    public int? CounterGoal { get; set; }
}
