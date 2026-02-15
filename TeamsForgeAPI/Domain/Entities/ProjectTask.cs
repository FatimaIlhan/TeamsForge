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
    public Guid? AssignedUserId { get; set; }  
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties - singular, not collections
    public Project Project { get; set; } = null!; // ONE Task belongs to ONE Project
    public ApplicationUser? AssignedUser { get; set; }  //one task has one assigned user
}

