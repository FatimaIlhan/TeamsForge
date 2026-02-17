using System;
using TeamsForgeAPI.Domain.Enums;

namespace TeamsForgeAPI.Domain.Entities;

public class Project
{
    public Guid ProjectId { get; set; }
    public Guid TeamId { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? CreatedById { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsArchived { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Team Team { get; set; } = null!;  // One project belongs to ONE team
    public ApplicationUser? CreatedByUser { get; set; }
    public ProjectCategory? Category { get; set; }
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();  // One project has MANY tasks
    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
}
