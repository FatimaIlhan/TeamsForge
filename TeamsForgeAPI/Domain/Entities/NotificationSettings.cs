using System;

namespace TeamsForgeAPI.Domain.Entities;

public class NotificationSettings
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool InAppNotifications { get; set; } = true;
    public bool TaskAssigned { get; set; } = true;
    public bool TaskCompleted { get; set; } = true;
    public bool CommentAdded { get; set; } = true;
    public bool Mentioned { get; set; } = true;
    public bool DueDateReminder { get; set; } = true;
    public bool TeamInvites { get; set; } = true;
    public bool DailyDigest { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
