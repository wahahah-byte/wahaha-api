using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wahaha.API.Models.Domain;

[Table("tasks")]
public class Task
{
    [Key]
    [Column("task_id")]
    public Guid TaskId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Column("priority")]
    public Priority Priority { get; set; }

    [Required]
    [Column("status")]
    public ByteTaskStatus Status { get; set; } = ByteTaskStatus.pending;

    [Column("point_value")]
    [Range(0, int.MaxValue)]
    public int PointValue { get; set; } = 10;

    [Column("due_date")]
    public DateTime? DueDate { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Required]
    [Column("is_recurring")]
    public bool IsRecurring { get; set; } = false;
    [Column("submitted")]
    public bool? Submitted { get; set; } = false;
    [MaxLength(50)]
    [Column("recurrence_rule")]
    public string? RecurrenceRule { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public Users? User { get; set; }
    public ICollection<Streak> Streaks { get; set; } = new List<Streak>();
}
public enum Priority
{
    LOW,
    MEDIUM,
    HIGH,
    CRITICAL
}
public enum ByteTaskStatus
{
    pending,
    in_progress,
    completed,
    cancelled
}