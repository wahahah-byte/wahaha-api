namespace wahaha.API.Models.DTO;

public class UpdateStreakDto
{
    public int StreakId { get; set; }
    public string StreakType { get; set; } = string.Empty;
    public int CurrentCount { get; set; }
    public int LongestCount { get; set; }
    public DateTime LastActivityDate { get; set; }
    public decimal BonusMultiplier { get; set; }
    public bool IsActive { get; set; }
    public Guid? TaskId { get; set; }

}
