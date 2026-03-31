namespace wahaha.API.Models.DTOs;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalPointsEarned { get; set; }
    public int CurrentBalance { get; set; }
    public int Level { get; set; }
    public int Xp { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PointTransactionDto> PointTransactions { get; set; } = new();
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalPointsEarned { get; set; }
    public int CurrentBalance { get; set; }
    public int Level { get; set; }
    public int Xp { get; set; }
}
