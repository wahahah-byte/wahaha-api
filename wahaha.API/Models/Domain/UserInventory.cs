using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wahaha.API.Models.Domain;

[Table("user_inventory")]
public class UserInventory
{
    [Key]
    [Column("inventory_id")]
    public int InventoryId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("item_id")]
    public int ItemId { get; set; }

    [Required]
    [Column("acquired_at")]
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("is_equipped")]
    public bool IsEquipped { get; set; } = false;

    // Personal inventory grid position; null = auto-place on first read. Mobile variant stored separately.
    [Column("position_x")]
    public int? PositionX { get; set; }

    [Column("position_y")]
    public int? PositionY { get; set; }

    [Column("position_x_mobile")]
    public int? PositionXMobile { get; set; }

    [Column("position_y_mobile")]
    public int? PositionYMobile { get; set; }

    // 90° rotation flag in inventory grid (desktop Q/E toggle); RN mobile doesn't expose it.
    [Required]
    [Column("is_rotated")]
    public bool IsRotated { get; set; } = false;

    // Navigation properties.
    [ForeignKey("UserId")]
    public Users? User { get; set; }

    [ForeignKey("ItemId")]
    public AvatarItem? AvatarItem { get; set; }
}
