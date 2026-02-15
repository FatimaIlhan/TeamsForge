using System;

namespace TeamsForgeAPI.Domain.Entities;

public class Attachment
{
    public Guid AttachmentId { get; set; }
    public Guid TaskId { get; set; }
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public Guid UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ProjectTask ProjectTask { get; set; } = null!;
    public ApplicationUser UploadedByUser { get; set; } = null!;
}
