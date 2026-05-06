namespace wahaha.API.Models.DTO;

public class SubtaskDto
{
    public int SubtaskId { get; set; }
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSubtaskRequest
{
    public string Title { get; set; } = string.Empty;
}

public class UpdateSubtaskRequest
{
    public string? Title { get; set; }
    public bool? Completed { get; set; }
    public int? SortOrder { get; set; }
}

public class ReorderSubtasksRequest
{
    public List<int> OrderedIds { get; set; } = new();
}
