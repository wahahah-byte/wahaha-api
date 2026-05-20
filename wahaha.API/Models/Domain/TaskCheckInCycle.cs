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

    // "checkin" = full check-in (cycle/points/streak); "log" = counter-only entry.
    [Required]
    [MaxLength(16)]
    [Column("cycle_type")]
    public string CycleType { get; set; } = "checkin";

    // Rollback snapshot populated only for checkin cycles.
    [Column("points_awarded")]
    public int? PointsAwarded { get; set; }

    [Column("point_transaction_id")]
    public int? PointTransactionId { get; set; }

    [Column("previous_due_date")]
    public DateTime? PreviousDueDate { get; set; }

    [Column("previous_last_check_in_date")]
    public DateTime? PreviousLastCheckInDate { get; set; }

    [Column("previous_streak_count")]
    public int? PreviousStreakCount { get; set; }

    [Column("previous_longest_count")]
    public int? PreviousLongestCount { get; set; }

    [Column("previous_streak_last_activity")]
    public DateTime? PreviousStreakLastActivity { get; set; }

    [Column("previous_streak_is_active")]
    public bool? PreviousStreakIsActive { get; set; }

    [Column("previous_streak_bonus_multiplier", TypeName = "decimal(18,4)")]
    public decimal? PreviousStreakBonusMultiplier { get; set; }

    [ForeignKey("TaskId")]
    public Task? Task { get; set; }
}
