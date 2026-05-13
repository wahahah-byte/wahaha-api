using System.ComponentModel.DataAnnotations;

namespace wahaha.API.Models.DTO;

public class AvatarItemDto
{
    public int ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Slot { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public int Cost { get; set; }
    public string? Description { get; set; }
    public string? PreviewAssetUrl { get; set; }
    public bool IsAvailable { get; set; }

    // Optional second blob URL — used for two-layer items where one row draws
    // a primary PNG (at its own slot's z-order) and a secondary PNG at the
    // HAIR_BACK z-order. Currently only HAIR_FRONT consumes this; null/empty
    // means the row is single-layer and behaves exactly like before.
    public string? SecondaryAssetUrl { get; set; }

    // Inventory footprint (in grid cells). Null falls back to 1x1 on the client.
    public int? GridCols { get; set; }
    public int? GridRows { get; set; }

    // Render hints — see AvatarItem.cs for documentation. All optional; the
    // client falls back to per-slot defaults when these are null.
    public bool? CoversHairFront { get; set; }
    public bool? CoversHairBack { get; set; }
    public int? OffsetX { get; set; }
    public int? OffsetY { get; set; }
    public double? RenderScale { get; set; }
    public int? SourceWidth { get; set; }
    public int? SourceHeight { get; set; }

    // Tight bounding box of non-transparent pixels in the source PNG, in
    // source-pixel coords (origin top-left). Populated by ContentBoundsService
    // at upload / register / recompute time. Consumed by the client's
    // boundsTransformFor() to auto-centre the visible content inside the
    // inventory card and admin row thumbnail. Null for legacy items that
    // pre-date the feature — client falls back to slot defaults.
    public int? ContentMinX { get; set; }
    public int? ContentMinY { get; set; }
    public int? ContentMaxX { get; set; }
    public int? ContentMaxY { get; set; }
}

public class CreateAvatarItemDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Slot { get; set; } = string.Empty;

    [Required]
    public string Rarity { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue)]
    public int Cost { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Image file — optional on create, can be added later via update
    public IFormFile? Image { get; set; }

    // Optional second image — uploaded to the same blob container and stored
    // in SecondaryAssetUrl. Only meaningful for slots whose render layer is
    // composed of two PNGs (HAIR_FRONT today). Null = single-layer row.
    public IFormFile? SecondaryImage { get; set; }

    // Optional render hints. See AvatarItem.cs for what each one does. All
    // nullable; the client falls back to per-slot defaults when unset.
    public bool? CoversHairFront { get; set; }
    public bool? CoversHairBack { get; set; }
    public int? OffsetX { get; set; }
    public int? OffsetY { get; set; }
    public double? RenderScale { get; set; }
    public int? SourceWidth { get; set; }
    public int? SourceHeight { get; set; }
}

// Used by the admin "register-by-url" endpoint when an asset is already
// uploaded to blob storage by hand. Skips the multipart upload path —
// PreviewAssetUrl is stored as-given.
public class RegisterAvatarItemByUrlDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Slot { get; set; } = string.Empty;

    [Required]
    public string Rarity { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Cost { get; set; } = 0;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    [Url]
    [MaxLength(255)]
    public string PreviewAssetUrl { get; set; } = string.Empty;

    // Optional second already-uploaded blob URL — see SecondaryAssetUrl on
    // AvatarItem.cs. Null/empty = single-layer row.
    [Url]
    [MaxLength(255)]
    public string? SecondaryAssetUrl { get; set; }

    public bool IsAvailable { get; set; } = true;

    // When true, also adds the item to the current user's inventory and
    // equips it (auto-unequipping anything else in the same slot).
    public bool GrantAndEquipForCurrentUser { get; set; } = false;

    // Optional render hints. See AvatarItem.cs for what each one does.
    public bool? CoversHairFront { get; set; }
    public bool? CoversHairBack { get; set; }
    public int? OffsetX { get; set; }
    public int? OffsetY { get; set; }
    public double? RenderScale { get; set; }
    public int? SourceWidth { get; set; }
    public int? SourceHeight { get; set; }
}

// Admin-only body for POST /api/AvatarItems/{id}/grant. TargetEmail is
// optional — when null/empty the item is granted to the calling admin.
public class GrantAvatarItemDto
{
    [EmailAddress]
    [MaxLength(100)]
    public string? TargetEmail { get; set; }

    public bool AutoEquip { get; set; } = false;
}

public class UpdateAvatarItemDto
{
    public int ItemId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public string Slot { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Cost { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    public bool IsAvailable { get; set; }

    // Optional — only upload a new image if provided
    public IFormFile? Image { get; set; }

    // Optional second image — when present the handler uploads it, deletes
    // the previous secondary blob (if any), and overwrites SecondaryAssetUrl.
    // When absent the existing SecondaryAssetUrl is preserved untouched.
    public IFormFile? SecondaryImage { get; set; }

    // Optional render hints. Sending null clears the field on the server side
    // (i.e. reverts the row to slot-default behaviour) — same as today's
    // GridCols/GridRows handling.
    public bool? CoversHairFront { get; set; }
    public bool? CoversHairBack { get; set; }
    public int? OffsetX { get; set; }
    public int? OffsetY { get; set; }
    public double? RenderScale { get; set; }
    public int? SourceWidth { get; set; }
    public int? SourceHeight { get; set; }
}
