using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wahaha.API.Models.Domain;

[Table("point_transactions")]
public class PointTransaction
{
    [Key]
    [Column("transaction_id")]
    public int TransactionId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("amount")]
    public int Amount { get; set; }

    [Required]
    [Column("type")]
    public TransactionType Type { get; set; }

    [Column("source_type")]
    public SourceType? SourceType { get; set; }

    [Column("source_id")]
    public int? SourceId { get; set; }

    [MaxLength(255)]
    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("UserId")]
    public Users? User { get; set; }
}
public enum TransactionType
{
    EARN,
    SPEND,
    BONUS
}

public enum SourceType
{
    task,
    minigame,
    shop_item,
    streak,
    achievement
}