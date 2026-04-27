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
    public int? CurrentStreakCount { get; set; }
    public int? LongestStreakCount { get; set; }
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
}
