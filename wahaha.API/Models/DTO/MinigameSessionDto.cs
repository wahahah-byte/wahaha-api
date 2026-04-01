namespace wahaha.API.Models.DTOs;

public class MinigameSessionDto
{
    public int SessionId { get; set; }
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public int GameId { get; set; }
    public string? GameName { get; set; }
    public int? Score { get; set; }
    public int PointsEarned { get; set; }
    public DateTime PlayedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string Outcome { get; set; } = string.Empty;
}

public class MinigameSessionLeaderboardDto
{
    public int SessionId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int GameId { get; set; }
    public string? GameName { get; set; }
    public int? Score { get; set; }
    public int PointsEarned { get; set; }
    public DateTime PlayedAt { get; set; }
    public string Outcome { get; set; } = string.Empty;
}

public class CreateMinigameSessionDto
{
    public Guid UserId { get; set; }
    public int GameId { get; set; }
    public int? Score { get; set; }
    public int PointsEarned { get; set; } = 0;
    public int? DurationSeconds { get; set; }
    public string Outcome { get; set; } = string.Empty;
}