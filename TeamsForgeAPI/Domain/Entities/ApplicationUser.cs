using System;
using Microsoft.AspNetCore.Identity;

namespace TeamsForgeAPI.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{

    //Custom fields 
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Multi-tenant support
    public Guid TenantId { get; set; }
    // Navigation properties 
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<ProjectTask> AssignedTasks { get; set; } = new List<ProjectTask>();//one user can have multiple assigned tasks
    public ICollection<Team> TeamsCreated { get; set; } = new List<Team>(); // Teams created by this user
    public ICollection<TeamUserRole> TeamUserRoles { get; set; } = new List<TeamUserRole>(); // Team memberships with roles
    // Refresh token support
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

}


