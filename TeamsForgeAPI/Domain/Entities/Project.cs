using System;

namespace TeamsForgeAPI.Domain.Entities;

public class Project
{
    public Guid ProjectId { get; set; }
    public Guid TeamId { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Team Team { get; set; } = null!;  // One project belongs to ONE team
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();  // One project has MANY tasks
}
