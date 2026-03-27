using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wahaha.API.Models.Domain;

[Table("minigames")]
public class Minigame
{
    [Key]
    [Column("game_id")]
    public Guid GameId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("max_points_reward")]
    [Range(0, int.MaxValue)]
    public int MaxPointsReward { get; set; }

    [Required]
    [Column("unlock_level")]
    [Range(1, int.MaxValue)]
    public int UnlockLevel { get; set; } = 1;

    [Required]
    [Column("type")]
    public GameType Type { get; set; }

    [Column("duration_seconds")]
    [Range(1, int.MaxValue)]
    public int? DurationSeconds { get; set; }

    [Required]
    [Column("difficulty")]
    public Difficulty Difficulty { get; set; }

    // Navigation property
    public ICollection<MinigameSession> Sessions { get; set; } = new List<MinigameSession>();
}
public enum Difficulty
{
    EASY,
    MEDIUM,
    HARD
}
public enum GameType
{
    arcade,
    quiz,
    puzzle,
    platformer,
    social,
    chance,
    coop
}