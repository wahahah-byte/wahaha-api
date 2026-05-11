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

    // RE-style inventory footprint. Nullable so old rows can fall back to the
    // client-side default of 1x1. The avatar page uses these to render each
    // item across (GridCols × GridRows) cells of the user's inventory grid.
    [Column("grid_cols")]
    public int? GridCols { get; set; }

    [Column("grid_rows")]
    public int? GridRows { get; set; }

    // Navigation property
    public ICollection<UserInventory> UserInventories { get; set; } = new List<UserInventory>();
}

public enum ItemSlot
{
    HEAD,
    HAIR,
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