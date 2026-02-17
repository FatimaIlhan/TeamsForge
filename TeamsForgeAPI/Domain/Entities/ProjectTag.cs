using System;

namespace TeamsForgeAPI.Domain.Entities;

public class ProjectTag
{
    public Guid ProjectId { get; set; }
    public Guid TagId { get; set; }

    public Project Project { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
