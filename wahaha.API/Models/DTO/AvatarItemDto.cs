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
}
