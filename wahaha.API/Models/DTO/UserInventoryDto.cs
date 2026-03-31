namespace wahaha.API.Models.DTOs;

public class UserInventoryDto
{
    public int InventoryId { get; set; }
    public Guid UserId { get; set; }
    public int ItemId { get; set; }
    public DateTime AcquiredAt { get; set; }
    public bool IsEquipped { get; set; }
    public AvatarItemDto? AvatarItem { get; set; }
}

public class CreateUserInventoryDto
{
    public Guid UserId { get; set; }
    public int ItemId { get; set; }
    public bool IsEquipped { get; set; } = false;
}
