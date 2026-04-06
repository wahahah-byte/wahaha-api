namespace wahaha.API.Models.DTO;

public class PointTransactionDto
{
    public int TransactionId { get; set; }
    public Guid UserId { get; set; }
    public int Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? SourceType { get; set; }
    public int? SourceId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePointTransactionDto
{
    public Guid UserId { get; set; }
    public int Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? SourceType { get; set; }
    public int? SourceId { get; set; }
    public string? Description { get; set; }
}
