namespace wahaha.API.Models.DTO;

public class CheckInResponse
{
    public int PointsAwarded { get; set; }
    public int NewBalance { get; set; }
    public int RecurringDailyTotal { get; set; }
    public int StreakCount { get; set; }
    public int LongestCount { get; set; }
    public decimal BonusMultiplier { get; set; }
    public bool StreakReset { get; set; }
    public string NextDueDate { get; set; } = string.Empty;
}

public class SubmitPointsRequest
{
    public List<string> TaskIds { get; set; } = new();
}

public class SubmitPointsResponse
{
    public int PointsAwarded { get; set; }
    public int NewBalance { get; set; }
    public int DailyTotal { get; set; }
    public int RecurringDailyTotal { get; set; }
    public List<PointTransactionDto> Transactions { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
