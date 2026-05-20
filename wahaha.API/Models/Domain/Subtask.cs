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

    // Fitness extension: SetsTarget triggers an X/Y set counter; row auto-completes at target.
    [Column("sets_target")]
    public int? SetsTarget { get; set; }

    [Column("reps_target")]
    public int? RepsTarget { get; set; }

    [Column("sets_completed")]
    public int? SetsCompleted { get; set; }

    [ForeignKey("TaskId")]
    public Task? Task { get; set; }
}
