using System;
using TeamsForgeAPI.Domain.Enums;

namespace TeamsForgeAPI.Domain.Entities;

public class TaskDependency
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid DependsOnTaskId { get; set; }
    public TaskDependencyType DependencyType { get; set; } = TaskDependencyType.Blocks;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProjectTask Task { get; set; } = null!;
    public ProjectTask DependsOnTask { get; set; } = null!;
}
