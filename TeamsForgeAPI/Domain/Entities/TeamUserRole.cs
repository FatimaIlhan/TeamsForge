using System;
using TeamsForgeAPI.Domain.Enums;

namespace TeamsForgeAPI.Domain.Entities;

public class TeamUserRole // Represents the association between a user, a team, and the user's role within that team
{
    public Guid UserId { get; set; }
    public Guid TeamId { get; set; }
    public TeamRole Role { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
