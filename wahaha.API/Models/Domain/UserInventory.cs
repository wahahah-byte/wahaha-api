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

    // Position in the user's personal grid (Resident-Evil-style placement).
    // Nullable so existing rows can be auto-placed on first read; the
    // frontend persists assigned positions back via PATCH /position.
    [Column("position_x")]
    public int? PositionX { get; set; }

    [Column("position_y")]
    public int? PositionY { get; set; }

    // Whether the item is currently rotated 90° in the inventory grid.
    // Frontend toggles this with Q/E while dragging; persists across reloads.
    [Required]
    [Column("is_rotated")]
    public bool IsRotated { get; set; } = false;

    // Navigation properties
    [ForeignKey("UserId")]
    public Users? User { get; set; }

    [ForeignKey("ItemId")]
    public AvatarItem? AvatarItem { get; set; }
}
