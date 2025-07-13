using Microsoft.AspNetCore.SignalR;
using OpenTask.Application.Interfaces;
using OpenTask.Domain.Entities;

namespace OpenTask.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(Guid userId, string message, string type)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            Type = type,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
        {
            notification.Id,
            notification.Message,
            notification.Type,
            notification.CreatedAt
        });
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        await Task.CompletedTask;
    }
}
