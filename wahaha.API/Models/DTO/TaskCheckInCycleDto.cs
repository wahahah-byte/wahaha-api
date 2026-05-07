namespace wahaha.API.Models.DTO;

public class CheckInCycleDto
{
    public int CycleId { get; set; }
    public Guid TaskId { get; set; }
    public DateTime CheckInDate { get; set; }
    public int? CounterValue { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CheckInRequest
{
    public int? CounterValue { get; set; }
}
