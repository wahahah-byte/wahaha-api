using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wahaha.API.Models.Domain;

[Table("users")]
public class Users
{
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("total_points_earned")]
    [Range(0, int.MaxValue)]
    public int TotalPointsEarned { get; set; } = 0;

    [Column("current_balance")]
    [Range(0, int.MaxValue)]
    public int CurrentBalance { get; set; } = 0;

    [Column("level")]
    [Range(1, int.MaxValue)]
    public int Level { get; set; } = 1;

    [Column("xp")]
    [Range(0, int.MaxValue)]
    public int Xp { get; set; } = 0;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Enforces the 13+ age gate at signup; nullable so the migration can backfill existing rows.
    [Column("date_of_birth")]
    public DateOnly? DateOfBirth { get; set; }

    // Public Azure Blob URL for the profile picture; null = client uses default avatar.
    [MaxLength(500)]
    [Column("profile_picture_url")]
    public string? ProfilePictureUrl { get; set; }

    // Navigation properties
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
    public ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();
    public ICollection<UserInventory> Inventory { get; set; } = new List<UserInventory>();
    public ICollection<MinigameSession> MinigameSessions { get; set; } = new List<MinigameSession>();
    public ICollection<Streak> Streaks { get; set; } = new List<Streak>();
}