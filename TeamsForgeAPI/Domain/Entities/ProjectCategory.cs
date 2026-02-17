using System;

namespace TeamsForgeAPI.Domain.Entities;

public class ProjectCategory
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string Name { get; set; } = null!;
    public string Color { get; set; } = "#808080";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Team Team { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
