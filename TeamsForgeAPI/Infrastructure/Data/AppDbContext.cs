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
        builder.Entity<Team>() .HasOne(t => t.CreatedByUser) .WithMany(u => u.TeamsCreated) .HasForeignKey(t => t.CreatedByUserId);

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

            // Store enum as string
            entity.Property(tur => tur.Role).HasConversion<string>();
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
    }
}