namespace wahaha.API.Models.DTO;

public class StreakDto
{
    public int StreakId { get; set; }
    public Guid UserId { get; set; }
    public string StreakType { get; set; } = string.Empty;
    public int CurrentCount { get; set; }
    public int LongestCount { get; set; }
    public DateTime LastActivityDate { get; set; }
    public decimal BonusMultiplier { get; set; }
    public bool IsActive { get; set; }
    public Guid? TaskId { get; set; }
}

public class CreateStreakDto
{
    public Guid UserId { get; set; }
    public string StreakType { get; set; } = string.Empty;
    public int CurrentCount { get; set; } = 0;
    public int LongestCount { get; set; } = 0;
    public decimal BonusMultiplier { get; set; } = 1.0m;
    public bool IsActive { get; set; } = true;
    public Guid? TaskId { get; set; }
}
