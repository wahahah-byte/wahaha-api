namespace wahaha.API.Models.DTOs;

public class MinigameDto
{
    public int GameId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxPointsReward { get; set; }
    public int UnlockLevel { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? DurationSeconds { get; set; }
    public string Difficulty { get; set; } = string.Empty;
}

public class CreateMinigameDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxPointsReward { get; set; }
    public int UnlockLevel { get; set; } = 1;
    public string Type { get; set; } = string.Empty;
    public int? DurationSeconds { get; set; }
    public string Difficulty { get; set; } = string.Empty;
}

public class UpdateMinigameDto
{
    public int GameId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxPointsReward { get; set; }
    public int UnlockLevel { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? DurationSeconds { get; set; }
    public string Difficulty { get; set; } = string.Empty;
}
