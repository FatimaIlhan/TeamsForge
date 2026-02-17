using System;

namespace TeamsForgeAPI.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string Name { get; set; } = null!;
    public string Color { get; set; } = "#808080";

    public Team Team { get; set; } = null!;
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
}
