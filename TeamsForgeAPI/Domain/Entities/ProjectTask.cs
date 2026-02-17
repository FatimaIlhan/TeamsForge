using System;
using TeamsForgeAPI.Domain.Enums;

namespace TeamsForgeAPI.Domain.Entities;

public class ProjectTask
{
    public Guid TaskId { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public ProjectTaskStatus Status { get; set; }  // Required enum, not nullable
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public Guid? AssignedUserId { get; set; }  
    public Guid? ReporterId { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int OrderIndex { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockedReason { get; set; }
    public Guid? ParentTaskId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties - singular, not collections
    public Project Project { get; set; } = null!; // ONE Task belongs to ONE Project
    public ApplicationUser? AssignedUser { get; set; }  //one task has one assigned user
    public ApplicationUser? Reporter { get; set; }
    public ProjectTask? ParentTask { get; set; }
    public ICollection<ProjectTask> Subtasks { get; set; } = new List<ProjectTask>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    public ICollection<TaskDependency> Dependencies { get; set; } = new List<TaskDependency>();
    public ICollection<TaskDependency> DependentOn { get; set; } = new List<TaskDependency>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}

