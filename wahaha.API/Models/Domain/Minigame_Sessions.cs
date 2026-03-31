using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wahaha.API.Models.Domain;

[Table("minigame_sessions")]
public class MinigameSession
{
    [Key]
    [Column("session_id")]
    public int SessionId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("game_id")]
    public int GameId { get; set; }

    [Column("score")]
    [Range(0, int.MaxValue)]
    public int? Score { get; set; }

    [Required]
    [Column("points_earned")]
    [Range(0, int.MaxValue)]
    public int PointsEarned { get; set; } = 0;

    [Required]
    [Column("played_at")]
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

    [Column("duration_seconds")]
    [Range(1, int.MaxValue)]
    public int? DurationSeconds { get; set; }

    [Required]
    [Column("outcome")]
    public SessionOutcome Outcome { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public Users? User { get; set; }

    [ForeignKey("GameId")]
    public Minigame? Minigame { get; set; }
}

public enum SessionOutcome
{
    WIN,
    LOSS,
    PARTIAL
}