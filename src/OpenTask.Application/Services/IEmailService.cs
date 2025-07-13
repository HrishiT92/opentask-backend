using OpenTask.Domain.Entities;

namespace OpenTask.Application.Services;

public interface IEmailService
{
    Task SendNotificationEmailAsync(User user, string subject, string message);
    Task SendTaskAssignedEmailAsync(User user, Issue issue);
    Task SendTaskStatusChangedEmailAsync(User user, Issue issue, string oldStatus, string newStatus);
    Task SendCommentNotificationEmailAsync(User user, Issue issue, Comment comment);
}
