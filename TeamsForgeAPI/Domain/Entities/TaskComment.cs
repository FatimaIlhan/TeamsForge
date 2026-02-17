using System;

namespace TeamsForgeAPI.Domain.Entities;

public class TaskComment
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsEdited { get; set; }

    public ProjectTask Task { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
