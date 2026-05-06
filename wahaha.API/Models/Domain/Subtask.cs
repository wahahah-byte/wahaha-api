using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wahaha.API.Models.Domain;

[Table("subtasks")]
public class Subtask
{
    [Key]
    [Column("subtask_id")]
    public int SubtaskId { get; set; }

    [Required]
    [Column("task_id")]
    public Guid TaskId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("completed")]
    public bool Completed { get; set; } = false;

    [Required]
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("TaskId")]
    public Task? Task { get; set; }
}
