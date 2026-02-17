using System;

namespace TeamsForgeAPI.Domain.Entities;

public class TimeEntry
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProjectTask Task { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
