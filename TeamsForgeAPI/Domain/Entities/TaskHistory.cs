using System;

namespace TeamsForgeAPI.Domain.Entities;

public class TaskHistory
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = null!;
    public string? Field { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProjectTask Task { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
