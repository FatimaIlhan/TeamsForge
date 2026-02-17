using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeamsForgeAPI.Domain.Entities;
namespace TeamsForgeAPI.Infrastructure.Data;


public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    // DbSets for your domain entities
    public DbSet<UserRole> AppUserRoles { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> Tasks { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<TeamUserRole> TeamUserRoles { get; set; }
    public DbSet<TaskHistory> TaskHistories { get; set; }
    public DbSet<TaskComment> TaskComments { get; set; }
    public DbSet<TeamInvitation> TeamInvitations { get; set; }
    public DbSet<ProjectCategory> ProjectCategories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TaskTag> TaskTags { get; set; }
    public DbSet<ProjectTag> ProjectTags { get; set; }
    public DbSet<NotificationSettings> NotificationSettings { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<TaskDependency> TaskDependencies { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure UserRole entity
        builder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        //  Team created by User 
        builder.Entity<Team>()
            .HasOne(t => t.CreatedByUser)
            .WithMany(u => u.TeamsCreated)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.DisplayName).HasMaxLength(200);
            entity.Property(u => u.AvatarUrl).HasMaxLength(500);
            // Unique index on Email
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Configure ProjectTask with composite index
        builder.Entity<ProjectTask>(entity =>
        {
            entity.HasKey(t => t.TaskId);
            // Composite index for filtering and sorting by ProjectId and Status
            entity.HasIndex(t => new { t.ProjectId, t.Status });
            entity.HasIndex(t => t.AssignedUserId);

            entity.HasOne(t => t.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Reporter)
                .WithMany(u => u.ReportedTasks)
                .HasForeignKey(t => t.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.ParentTask)
                .WithMany(t => t.Subtasks)
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Team with index on Name
        builder.Entity<Team>(entity =>
        {
            // Index for quick lookup by Name
            entity.HasIndex(t => t.Name);
        });

        // Configure TeamUserRole entity (bridge for team membership with roles)
        builder.Entity<TeamUserRole>(entity =>
        {
            // Composite primary key
            entity.HasKey(tur => new { tur.UserId, tur.TeamId });

            // Foreign key to ApplicationUser
            entity.HasOne(tur => tur.User)
                .WithMany(u => u.TeamUserRoles)
                .HasForeignKey(tur => tur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign key to Team
            entity.HasOne(tur => tur.Team)
                .WithMany(t => t.TeamUserRoles)
                .HasForeignKey(tur => tur.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Attachment relationships
        builder.Entity<Attachment>(entity =>
        {
            entity.HasOne(a => a.ProjectTask)
                .WithMany()
                .HasForeignKey(a => a.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.UploadedByUser)
                .WithMany()
                .HasForeignKey(a => a.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Project>(entity =>
        {
            entity.HasIndex(p => new { p.TeamId, p.Status });
            entity.HasIndex(p => p.CategoryId);

            entity.HasOne(p => p.CreatedByUser)
                .WithMany(u => u.ProjectsCreated)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TaskHistory>(entity =>
        {
            entity.HasIndex(th => th.TaskId);
            entity.HasIndex(th => th.UserId);
            entity.HasIndex(th => th.CreatedAt);

            entity.HasOne(th => th.Task)
                .WithMany(t => t.History)
                .HasForeignKey(th => th.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(th => th.User)
                .WithMany(u => u.TaskHistories)
                .HasForeignKey(th => th.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TaskComment>(entity =>
        {
            entity.HasIndex(tc => tc.TaskId);
            entity.HasIndex(tc => tc.UserId);
            entity.HasIndex(tc => tc.CreatedAt);

            entity.HasOne(tc => tc.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tc => tc.User)
                .WithMany(u => u.TaskComments)
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TeamInvitation>(entity =>
        {
            entity.HasIndex(ti => ti.TeamId);
            entity.HasIndex(ti => ti.Email);
            entity.HasIndex(ti => ti.Status);
            entity.HasIndex(ti => ti.Token).IsUnique();
            entity.HasIndex(ti => new { ti.TeamId, ti.Email }).IsUnique();

            entity.HasOne(ti => ti.Team)
                .WithMany(t => t.Invitations)
                .HasForeignKey(ti => ti.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ti => ti.CreatedByUser)
                .WithMany(u => u.InvitationsCreated)
                .HasForeignKey(ti => ti.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectCategory>(entity =>
        {
            entity.HasIndex(pc => new { pc.TeamId, pc.Name }).IsUnique();

            entity.HasOne(pc => pc.Team)
                .WithMany(t => t.ProjectCategories)
                .HasForeignKey(pc => pc.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Tag>(entity =>
        {
            entity.HasIndex(t => new { t.TeamId, t.Name }).IsUnique();

            entity.HasOne(t => t.Team)
                .WithMany(team => team.Tags)
                .HasForeignKey(t => t.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TaskTag>(entity =>
        {
            entity.HasKey(tt => new { tt.TaskId, tt.TagId });

            entity.HasOne(tt => tt.Task)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tt => tt.Tag)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectTag>(entity =>
        {
            entity.HasKey(pt => new { pt.ProjectId, pt.TagId });

            entity.HasOne(pt => pt.Project)
                .WithMany(p => p.ProjectTags)
                .HasForeignKey(pt => pt.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pt => pt.Tag)
                .WithMany(t => t.ProjectTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<NotificationSettings>(entity =>
        {
            entity.HasIndex(ns => ns.UserId).IsUnique();

            entity.HasOne(ns => ns.User)
                .WithOne(u => u.NotificationSettings)
                .HasForeignKey<NotificationSettings>(ns => ns.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ActivityLog>(entity =>
        {
            entity.HasIndex(al => al.UserId);
            entity.HasIndex(al => al.EntityType);
            entity.HasIndex(al => al.CreatedAt);

            entity.HasOne(al => al.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<TaskDependency>(entity =>
        {
            entity.HasIndex(td => td.TaskId);
            entity.HasIndex(td => td.DependsOnTaskId);
            entity.HasIndex(td => new { td.TaskId, td.DependsOnTaskId }).IsUnique();
            entity.HasCheckConstraint("CHK_NoSelfDependency", "[TaskId] <> [DependsOnTaskId]");

            entity.HasOne(td => td.Task)
                .WithMany(t => t.Dependencies)
                .HasForeignKey(td => td.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(td => td.DependsOnTask)
                .WithMany(t => t.DependentOn)
                .HasForeignKey(td => td.DependsOnTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TimeEntry>(entity =>
        {
            entity.HasIndex(te => te.TaskId);
            entity.HasIndex(te => te.UserId);
            entity.HasIndex(te => te.EntryDate);

            entity.HasOne(te => te.Task)
                .WithMany(t => t.TimeEntries)
                .HasForeignKey(te => te.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(te => te.User)
                .WithMany(u => u.TimeEntries)
                .HasForeignKey(te => te.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ApiKey>(entity =>
        {
            entity.HasIndex(ak => ak.UserId);
            entity.HasIndex(ak => ak.Key).IsUnique();

            entity.HasOne(ak => ak.User)
                .WithMany(u => u.ApiKeys)
                .HasForeignKey(ak => ak.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}