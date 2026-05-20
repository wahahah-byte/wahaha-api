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

    // Optional secondary blob URL for two-layer items (HAIR_FRONT today); null = single-layer.
    public string? SecondaryAssetUrl { get; set; }

    // Inventory footprint in grid cells; null = 1x1 client-side.
    public int? GridCols { get; set; }
    public int? GridRows { get; set; }

    // Optional render hints; null = use per-slot defaults client-side.
    public bool? CoversHairFront { get; set; }
    public bool? CoversHairBack { get; set; }
    public int? OffsetX { get; set; }
    public int? OffsetY { get; set; }
    public double? RenderScale { get; set; }
    public int? SourceWidth { get; set; }
    public int? SourceHeight { get; set; }

    // Tight bbox of non-transparent pixels in source-pixel coords; null = slot defaults.
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

    // Optional second image for two-layer items (HAIR_FRONT today); null = single-layer.
    public IFormFile? SecondaryImage { get; set; }

    // Optional render hints; null = use per-slot defaults client-side.
    public bool? CoversHairFront { get; set; }
    public bool? CoversHairBack { get; set; }
    public int? OffsetX { get; set; }
    public int? OffsetY { get; set; }
    public double? RenderScale { get; set; }
    public int? SourceWidth { get; set; }
    public int? SourceHeight { get; set; }
}

// Admin register-by-url body for assets already in blob storage.
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

    // Optional second already-uploaded blob URL; null = single-layer row.
    [Url]
    [MaxLength(255)]
    public string? SecondaryAssetUrl { get; set; }

    public bool IsAvailable { get; set; } = true;

    // When true, also grants + equips on the current user (auto-unequip slot rivals).
    public bool GrantAndEquipForCurrentUser { get; set; } = false;

    // Optional render hints.
    public bool? CoversHairFront { get; set; }
    public bool? CoversHairBack { get; set; }
    public int? OffsetX { get; set; }
    public int? OffsetY { get; set; }
    public double? RenderScale { get; set; }
    public int? SourceWidth { get; set; }
    public int? SourceHeight { get; set; }
}

// Admin grant body; empty TargetEmail = grant to caller.
public class GrantAvatarItemDto
{
    [EmailAddress]
    [MaxLength(100)]
    public string? TargetEmail { get; set; }

    public bool AutoEquip { get; set; } = false;
}

// Purchase body; UserId comes from JWT so caller can only buy for self.
public class PurchaseAvatarItemDto
{
    public bool AutoEquip { get; set; } = false;
}

// Purchase response: new inventory row + post-purchase balance in one shot.
public class PurchaseAvatarItemResponseDto
{
    public UserInventoryDto Inventory { get; set; } = null!;
    public int NewBalance { get; set; }
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

    // Optional new image upload.
    public IFormFile? Image { get; set; }

    // Optional new secondary image; absent leaves existing SecondaryAssetUrl untouched.
    public IFormFile? SecondaryImage { get; set; }

    // Optional render hints; null clears the field server-side.
    public bool? CoversHairFront { get; set; }
    public bool? CoversHairBack { get; set; }
    public int? OffsetX { get; set; }
    public int? OffsetY { get; set; }
    public double? RenderScale { get; set; }
    public int? SourceWidth { get; set; }
    public int? SourceHeight { get; set; }
}
