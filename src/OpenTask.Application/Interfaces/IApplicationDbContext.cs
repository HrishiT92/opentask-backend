using Microsoft.EntityFrameworkCore;
using OpenTask.Domain.Entities;

namespace OpenTask.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<Issue> Issues { get; }
    DbSet<Sprint> Sprints { get; }
    DbSet<Comment> Comments { get; }
    DbSet<IssueAttachment> IssueAttachments { get; }
    DbSet<IssueLabel> IssueLabels { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IAuthService
{
    Task<string> GenerateJwtTokenAsync(User user);
    Task<string> GenerateRefreshTokenAsync();
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> DownloadFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
}

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string message, string type);
    Task SendEmailAsync(string to, string subject, string body);
}
