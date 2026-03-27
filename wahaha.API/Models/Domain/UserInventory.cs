using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using wahaha.API.Models.Domain;
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

    // Navigation properties
    [ForeignKey("UserId")]
    public Users? User { get; set; }

    [ForeignKey("ItemId")]
    public AvatarItem? AvatarItem { get; set; }
}
