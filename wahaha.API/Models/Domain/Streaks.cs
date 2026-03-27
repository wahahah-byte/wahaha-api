using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wahaha.API.Models.Domain;

[Table("streaks")]
public class Streak
{
    [Key]
    [Column("streak_id")]
    public Guid StreakId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("streak_type")]
    public string StreakType { get; set; } = string.Empty;

    [Required]
    [Column("current_count")]
    [Range(0, int.MaxValue)]
    public int CurrentCount { get; set; } = 0;

    [Required]
    [Column("longest_count")]
    [Range(0, int.MaxValue)]
    public int LongestCount { get; set; } = 0;

    [Required]
    [Column("last_activity_date")]
    public DateTime LastActivityDate { get; set; }

    [Required]
    [Column("bonus_multiplier", TypeName = "decimal(18,4)")]
    [Range(1.0, double.MaxValue)]
    public decimal BonusMultiplier { get; set; } = 1.0m;

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // Navigation property
    [ForeignKey("UserId")]
    public Users? User { get; set; }
}