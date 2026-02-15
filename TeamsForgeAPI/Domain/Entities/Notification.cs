using System;

namespace TeamsForgeAPI.Domain.Entities;

public class Notification
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
