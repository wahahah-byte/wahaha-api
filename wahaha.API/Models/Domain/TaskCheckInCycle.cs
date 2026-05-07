using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wahaha.API.Models.Domain;

[Table("task_check_in_cycles")]
public class TaskCheckInCycle
{
    [Key]
    [Column("cycle_id")]
    public int CycleId { get; set; }

    [Required]
    [Column("task_id")]
    public Guid TaskId { get; set; }

    [Required]
    [Column("check_in_date")]
    public DateTime CheckInDate { get; set; }

    [Column("counter_value")]
    public int? CounterValue { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("TaskId")]
    public Task? Task { get; set; }
}
