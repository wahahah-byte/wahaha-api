namespace wahaha.API.Models.DTO;

public class UserInventoryDto
{
    public int InventoryId { get; set; }
    public Guid UserId { get; set; }
    public int ItemId { get; set; }
    public DateTime AcquiredAt { get; set; }
    public bool IsEquipped { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    // Mobile-shape (5×7) placement. Stored separately from PositionX/Y so
    // the desktop (7×5) and mobile (5×7) layouts can each persist without
    // overwriting the other.
    public int? PositionXMobile { get; set; }
    public int? PositionYMobile { get; set; }
    public bool IsRotated { get; set; }
    public AvatarItemDto? AvatarItem { get; set; }
}

public class CreateUserInventoryDto
{
    public Guid UserId { get; set; }
    public int ItemId { get; set; }
    public bool IsEquipped { get; set; } = false;
}

// Sent to PATCH /api/UserInventory/{id}/position to relocate a single item.
// Null values clear the position (item drops back into auto-placement).
// IsRotated is optional — only applied when present in the payload, so the
// existing position-only callers keep working untouched.
// Layout selects which placement pair to update: "desktop" (default) writes
// PositionX/Y, "mobile" writes PositionXMobile/YMobile. The field is
// optional and defaults to desktop so existing callers keep working.
public class UpdateInventoryPositionDto
{
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    public bool? IsRotated { get; set; }
    public string? Layout { get; set; }
}
