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
public class UpdateInventoryPositionDto
{
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
}
