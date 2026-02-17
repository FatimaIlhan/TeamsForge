using System;
using TeamsForgeAPI.Domain.Enums;

namespace TeamsForgeAPI.Domain.Entities;

public class TeamInvitation
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public TeamRole Role { get; set; } = TeamRole.TeamMember;
    public DateTime ExpiresAt { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedById { get; set; }
    public DateTime? AcceptedAt { get; set; }

    public Team Team { get; set; } = null!;
    public ApplicationUser CreatedByUser { get; set; } = null!;
}
