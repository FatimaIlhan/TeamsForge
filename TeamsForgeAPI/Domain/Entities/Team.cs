using System;

namespace TeamsForgeAPI.Domain.Entities;

public class Team
{
    public Guid TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Foreign key to ApplicationUser
    public Guid CreatedByUserId { get; set; }

    // Navigation property 
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<TeamUserRole> TeamUserRoles { get; set; } = new List<TeamUserRole>();
}
