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
    public DateTime? LastLoginAt { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Language { get; set; } = "en";
    public string? Preferences { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Multi-tenant support
    public Guid TenantId { get; set; }
    // Navigation properties 
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<ProjectTask> AssignedTasks { get; set; } = new List<ProjectTask>();//one user can have multiple assigned tasks
    public ICollection<ProjectTask> ReportedTasks { get; set; } = new List<ProjectTask>();
    public ICollection<Team> TeamsCreated { get; set; } = new List<Team>(); // Teams created by this user
    public ICollection<Project> ProjectsCreated { get; set; } = new List<Project>();
    public ICollection<TeamUserRole> TeamUserRoles { get; set; } = new List<TeamUserRole>(); // Team memberships with roles
    public ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
    public ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
    public ICollection<TeamInvitation> InvitationsCreated { get; set; } = new List<TeamInvitation>();
    public NotificationSettings? NotificationSettings { get; set; }
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    // Refresh token support
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

}


