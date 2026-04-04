using wahaha.API.Models.Pagination;

namespace wahaha.API.Models.Filters;

public class UserFilterParams : PaginationParams
{
    public int? Level { get; set; }
    public int? MinLevel { get; set; }
    public int? MaxLevel { get; set; }
}

public class AvatarItemFilterParams : PaginationParams
{
    public string? Rarity { get; set; }
    public string? Slot { get; set; }
    public int? MinCost { get; set; }
    public int? MaxCost { get; set; }
    public bool? IsAvailable { get; set; }
}

public class MinigameFilterParams : PaginationParams
{
    public string? Difficulty { get; set; }
    public string? Type { get; set; }
    public int? MinUnlockLevel { get; set; }
    public int? MaxUnlockLevel { get; set; }
}

public class MinigameSessionFilterParams : PaginationParams
{
    public Guid? UserId { get; set; }
    public int? GameId { get; set; }
    public string? Outcome { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class PointTransactionFilterParams : PaginationParams
{
    public Guid? UserId { get; set; }
    public string? Type { get; set; }
    public string? SourceType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class StreakFilterParams : PaginationParams
{
    public Guid? UserId { get; set; }
    public string? StreakType { get; set; }
    public bool? IsActive { get; set; }
    public int? MinCount { get; set; }
}

public class TaskFilterParams : PaginationParams
{
    public Guid? UserId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public bool? IsRecurring { get; set; }
}

public class UserInventoryFilterParams : PaginationParams
{
    public Guid? UserId { get; set; }
    public bool? IsEquipped { get; set; }
    public string? Slot { get; set; }
    public string? Rarity { get; set; }
}