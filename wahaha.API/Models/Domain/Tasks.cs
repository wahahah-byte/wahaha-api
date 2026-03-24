namespace wahaha.API.Models.Domain;

public class Tasks
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
    public int PointValue { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime CompletedDate { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType Recurrence { get; set; }
}

public enum RecurrenceType
{
    DAILY,
    WEEKLY,
    YEARLY
}