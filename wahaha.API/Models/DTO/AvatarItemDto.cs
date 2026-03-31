namespace wahaha.API.Models.DTOs;

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
}

public class CreateAvatarItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Slot { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public int Cost { get; set; }
    public string? Description { get; set; }
    public string? PreviewAssetUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public class UpdateAvatarItemDto
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
}
