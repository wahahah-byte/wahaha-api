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

    // Optional second PNG used for two-layer items where one row drives two
    // render layers — currently only HAIR_FRONT items that also draw a back
    // layer (the strands behind the head). Rendered by ChibiAvatar at
    // HAIR_BACK z-order; the inventory card still shows only PreviewAssetUrl.
    [MaxLength(255)]
    [Column("secondary_asset_url")]
    public string? SecondaryAssetUrl { get; set; }

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

    // ---- Render hints (chibi composite tuning) -------------------------------
    // Optional per-item knobs the chibi rendering layer uses to position the
    // PNG correctly over base.png. All nullable — when unset, the frontend
    // falls back to per-slot defaults (SLOT_TRANSFORM / SLOT_Z). Surfaced
    // through the admin "Advanced" panel in AvatarItemFormModal so artists
    // can dial them in per asset without code edits. These used to live in a
    // frontend RENDER_HINTS dictionary keyed by filename.

    // Hides HAIR_FRONT (bangs) when this item is equipped — e.g. a helmet
    // that fully covers the front of the head.
    [Column("covers_hair_front")]
    public bool? CoversHairFront { get; set; }

    // Hides HAIR_BACK when this item is equipped — uncommon, used for full
    // headpieces that wrap the back of the skull.
    [Column("covers_hair_back")]
    public bool? CoversHairBack { get; set; }

    // Pixel nudges in source-canvas coordinates (relative to a 256-wide /
    // 384-tall canvas; rescaled to the rendered chibi size client-side).
    // Positive = right / down. Use for sprites whose visual anchor sits a
    // few pixels off the canvas centreline.
    [Column("offset_x")]
    public int? OffsetX { get; set; }

    [Column("offset_y")]
    public int? OffsetY { get; set; }

    // Uniform scale applied around the item's centre. 1.0 = no scaling.
    // Typical range 0.5–2.5. Use to enlarge a small sprite or shrink one
    // drawn at a larger size than the canvas region it occupies.
    [Column("render_scale")]
    public double? RenderScale { get; set; }

    // Native canvas dimensions of the asset. Default to 256×384 (matching
    // base.png) when null. Set when a sprite extends past the character
    // bounds — e.g. an oversized weapon drawn on a 384×384 canvas.
    [Column("source_width")]
    public int? SourceWidth { get; set; }

    [Column("source_height")]
    public int? SourceHeight { get; set; }

    // ---- Content bounding box (auto-centering for inventory cards) -----------
    // Tight bbox of non-transparent pixels in the uploaded PNG, computed at
    // upload time via ImageSharp. The frontend uses these to translate/scale
    // the source so its visible content centres inside the inventory card,
    // instead of relying on hand-tuned per-slot defaults in SLOT_TRANSFORM.
    // All nullable: existing rows (and items registered by external URL,
    // where the asset isn't in our blob storage) fall back to slot defaults.

    [Column("content_min_x")]
    public int? ContentMinX { get; set; }

    [Column("content_min_y")]
    public int? ContentMinY { get; set; }

    [Column("content_max_x")]
    public int? ContentMaxX { get; set; }

    [Column("content_max_y")]
    public int? ContentMaxY { get; set; }

    // Navigation property
    public ICollection<UserInventory> UserInventories { get; set; } = new List<UserInventory>();
}

// EF stores enum values as their underlying int by default. The legacy 7
// slots are kept at positions 0-6 so existing rows (seeded HEAD/HAIR/BODY/
// HAND/FACE/BACK/FEET items) keep validating without a data migration.
// New uploads should use the granular MapleStory-style slots below — the
// admin UI offers those exclusively in the slot dropdown going forward.
public enum ItemSlot
{
    // ---- Legacy aliases (deprecated for new uploads, kept for compat) ----
    HEAD = 0,
    HAIR = 1,
    BODY = 2,
    HAND = 3,
    FACE = 4,
    BACK = 5,
    FEET = 6,

    // ---- Granular slots, MapleStory-style -------------------------------
    // Ordered roughly back-to-front so the int value implies render z-order
    // (the frontend overrides z-order explicitly anyway, this is just for
    // human readability of stored values).
    WEAPON_BACK  = 7,
    CAPE         = 8,
    HAIR_BACK    = 9,
    BOTTOM       = 10,
    TOP          = 11,
    // OVERALL is a one-piece — when equipped, the chibi composite suppresses
    // any equipped TOP and BOTTOM behind it. The user can still own + equip
    // a TOP/BOTTOM underneath; they're just visually hidden.
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