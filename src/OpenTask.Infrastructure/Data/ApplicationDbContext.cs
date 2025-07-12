using Microsoft.EntityFrameworkCore;
using OpenTask.Application.Interfaces;
using OpenTask.Domain.Entities;

namespace OpenTask.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<IssueAttachment> IssueAttachments => Set<IssueAttachment>();
    public DbSet<IssueLabel> IssueLabels => Set<IssueLabel>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(10);
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.ProjectId }).IsUnique();
        });

        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasOne(i => i.Project)
                .WithMany(p => p.Issues)
                .HasForeignKey(i => i.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Assignee)
                .WithMany(u => u.AssignedIssues)
                .HasForeignKey(i => i.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(i => i.Sprint)
                .WithMany(s => s.Issues)
                .HasForeignKey(i => i.SprintId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(i => i.ParentIssue)
                .WithMany(i => i.SubIssues)
                .HasForeignKey(i => i.ParentIssueId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
        });

        modelBuilder.Entity<Sprint>(entity =>
        {
            entity.HasOne(s => s.Project)
                .WithMany(p => p.Sprints)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasOne(c => c.Issue)
                .WithMany(i => i.Comments)
                .HasForeignKey(c => c.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.Content).IsRequired();
        });

        modelBuilder.Entity<IssueAttachment>(entity =>
        {
            entity.HasOne(ia => ia.Issue)
                .WithMany(i => i.Attachments)
                .HasForeignKey(ia => ia.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<IssueLabel>(entity =>
        {
            entity.HasOne(il => il.Project)
                .WithMany()
                .HasForeignKey(il => il.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(il => il.Issues)
                .WithMany(i => i.Labels);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(7);
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(al => al.Project)
                .WithMany()
                .HasForeignKey(al => al.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
        });
    }
}
