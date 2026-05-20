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

    // Optional second PNG for two-layer items (HAIR_FRONT today); drawn at HAIR_BACK z-order.
    [MaxLength(255)]
    [Column("secondary_asset_url")]
    public string? SecondaryAssetUrl { get; set; }

    [Required]
    [Column("is_available")]
    public bool IsAvailable { get; set; } = true;

    // RE-style inventory footprint; null falls back to 1x1 client-side.
    [Column("grid_cols")]
    public int? GridCols { get; set; }

    [Column("grid_rows")]
    public int? GridRows { get; set; }

    // Render hints (chibi composite tuning); null = use per-slot defaults client-side.

    // Hides HAIR_FRONT (bangs) when equipped.
    [Column("covers_hair_front")]
    public bool? CoversHairFront { get; set; }

    // Hides HAIR_BACK when equipped.
    [Column("covers_hair_back")]
    public bool? CoversHairBack { get; set; }

    // Pixel nudges in source-canvas coords (positive = right/down).
    [Column("offset_x")]
    public int? OffsetX { get; set; }

    [Column("offset_y")]
    public int? OffsetY { get; set; }

    // Uniform scale around item centre (1.0 = none).
    [Column("render_scale")]
    public double? RenderScale { get; set; }

    // Native canvas dimensions of the asset; default 256×384 when null.
    [Column("source_width")]
    public int? SourceWidth { get; set; }

    [Column("source_height")]
    public int? SourceHeight { get; set; }

    // Tight bbox of non-transparent pixels, computed via ImageSharp at upload time.
    [Column("content_min_x")]
    public int? ContentMinX { get; set; }

    [Column("content_min_y")]
    public int? ContentMinY { get; set; }

    [Column("content_max_x")]
    public int? ContentMaxX { get; set; }

    [Column("content_max_y")]
    public int? ContentMaxY { get; set; }

    // Navigation property.
    public ICollection<UserInventory> UserInventories { get; set; } = new List<UserInventory>();
}

// Legacy slots 0-6 kept for compat; new uploads use the granular MapleStory-style slots below.
public enum ItemSlot
{
    // Legacy aliases (deprecated for new uploads).
    HEAD = 0,
    HAIR = 1,
    BODY = 2,
    HAND = 3,
    FACE = 4,
    BACK = 5,
    FEET = 6,

    // Granular slots, ordered roughly back-to-front for readability.
    WEAPON_BACK  = 7,
    CAPE         = 8,
    HAIR_BACK    = 9,
    BOTTOM       = 10,
    TOP          = 11,
    // OVERALL is a one-piece; suppresses equipped TOP/BOTTOM visually but keeps them owned.
    OVERALL      = 12,
    GLOVES       = 13,
    SHOES        = 14,
    HAIR_FRONT   = 15,
    EYE          = 16,
    EAR          = 17,
    HAT          = 18,
    WEAPON_FRONT = 19,
    WRIST        = 20,
}
public enum Rarity
{
    COMMON,
    UNCOMMON,
    RARE,
    EPIC,
    LEGENDARY
}