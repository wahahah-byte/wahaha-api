namespace wahaha.API.Models.DTO;

public class SubmitPointsRequest
{
    public List<string> TaskIds { get; set; } = new();
}

public class SubmitPointsResponse
{
    public int PointsAwarded { get; set; }
    public int NewBalance { get; set; }
    public int DailyTotal { get; set; }
    public List<PointTransactionDto> Transactions { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
