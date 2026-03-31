using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wahaha.API.Models.Domain;

[Table("avatar_items")]
public class AvatarItem
{
    [Key]
    [Column("item_id")]
    public int ItemId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Column("slot")]
    public ItemSlot Slot { get; set; }

    [Required]
    [Column("rarity")]
    public Rarity Rarity { get; set; }

    [Required]
    [Column("cost")]
    [Range(0, int.MaxValue)]
    public int Cost { get; set; }

    [MaxLength(255)]
    [Column("description")]
    public string? Description { get; set; }

    [MaxLength(255)]
    [Column("preview_asset_url")]
    public string? PreviewAssetUrl { get; set; }

    [Required]
    [Column("is_available")]
    public bool IsAvailable { get; set; } = true;

    // Navigation property
    public ICollection<UserInventory> UserInventories { get; set; } = new List<UserInventory>();
}

public enum ItemSlot
{
    HEAD,
    BODY,
    HAND,
    FACE,
    BACK,
    FEET
}
public enum Rarity
{
    COMMON,
    UNCOMMON,
    RARE,
    EPIC,
    LEGENDARY
}