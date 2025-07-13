using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTask.Application.Services;
using OpenTask.Domain.Entities;
using System.Net;
using System.Net.Mail;

namespace OpenTask.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendNotificationEmailAsync(User user, string subject, string message)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"] ?? "localhost";
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
            var username = smtpSettings["Username"] ?? "";
            var password = smtpSettings["Password"] ?? "";
            var fromEmail = smtpSettings["FromEmail"] ?? "noreply@opentask.com";

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(username, password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "OpenTask"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(user.Email);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", user.Email);
        }
    }

    public async Task SendTaskAssignedEmailAsync(User user, Issue issue)
    {
        var subject = $"Task Assigned: {issue.Title}";
        var message = $@"
            <h2>You have been assigned a new task</h2>
            <p><strong>Title:</strong> {issue.Title}</p>
            <p><strong>Description:</strong> {issue.Description}</p>
            <p><strong>Priority:</strong> {issue.Priority}</p>
            <p><strong>Due Date:</strong> {issue.DueDate?.ToString("yyyy-MM-dd") ?? "Not set"}</p>
        ";

        await SendNotificationEmailAsync(user, subject, message);
    }

    public async Task SendTaskStatusChangedEmailAsync(User user, Issue issue, string oldStatus, string newStatus)
    {
        var subject = $"Task Status Changed: {issue.Title}";
        var message = $@"
            <h2>Task status has been updated</h2>
            <p><strong>Title:</strong> {issue.Title}</p>
            <p><strong>Status changed from:</strong> {oldStatus} → {newStatus}</p>
            <p><strong>Description:</strong> {issue.Description}</p>
        ";

        await SendNotificationEmailAsync(user, subject, message);
    }

    public async Task SendCommentNotificationEmailAsync(User user, Issue issue, Comment comment)
    {
        var subject = $"New Comment: {issue.Title}";
        var message = $@"
            <h2>New comment on task</h2>
            <p><strong>Task:</strong> {issue.Title}</p>
            <p><strong>Comment:</strong> {comment.Content}</p>
            <p><strong>Author:</strong> {comment.Author?.FirstName} {comment.Author?.LastName}</p>
        ";

        await SendNotificationEmailAsync(user, subject, message);
    }
}
