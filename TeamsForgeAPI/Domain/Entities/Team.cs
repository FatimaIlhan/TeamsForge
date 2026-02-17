using System;

namespace TeamsForgeAPI.Domain.Entities;

public class Team
{
    public Guid TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public string? Settings { get; set; }
    public string? AvatarUrl { get; set; }
    // Foreign key to ApplicationUser
    public Guid CreatedByUserId { get; set; }

    // Navigation property 
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<TeamUserRole> TeamUserRoles { get; set; } = new List<TeamUserRole>();
    public ICollection<ProjectCategory> ProjectCategories { get; set; } = new List<ProjectCategory>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<TeamInvitation> Invitations { get; set; } = new List<TeamInvitation>();
}
