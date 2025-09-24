using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using EnterpriseTicketing.Data;
using EnterpriseTicketing.Models;
using EnterpriseTicketing.Hubs;

namespace EnterpriseTicketing.Services;

public interface INotificationService
{
    Task SendNotificationAsync(int userId, string title, string message, NotificationType type = NotificationType.Info, int? relatedTicketId = null, int? sentByUserId = null);
    Task SendNotificationToAllUsersAsync(string title, string message, NotificationType type = NotificationType.Info, int? sentByUserId = null);
    Task SendNotificationToRoleAsync(UserRole role, string title, string message, NotificationType type = NotificationType.Info, int? sentByUserId = null);
    Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationAsync(int userId, string title, string message, NotificationType type = NotificationType.Info, int? relatedTicketId = null, int? sentByUserId = null)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedTicketId = relatedTicketId,
                SentByUserId = sentByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification via SignalR
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type.ToString(),
                CreatedAt = notification.CreatedAt,
                RelatedTicketId = notification.RelatedTicketId,
                IsRead = notification.IsRead
            });

            _logger.LogInformation("Notification sent to user {UserId}: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
        }
    }

    public async Task SendNotificationToAllUsersAsync(string title, string message, NotificationType type = NotificationType.Info, int? sentByUserId = null)
    {
        try
        {
            var users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            
            foreach (var user in users)
            {
                await SendNotificationAsync(user.Id, title, message, type, null, sentByUserId);
            }

            _logger.LogInformation("Notification sent to all users: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to all users");
        }
    }

    public async Task SendNotificationToRoleAsync(UserRole role, string title, string message, NotificationType type = NotificationType.Info, int? sentByUserId = null)
    {
        try
        {
            var users = await _context.Users.Where(u => u.IsActive && u.Role == role).ToListAsync();
            
            foreach (var user in users)
            {
                await SendNotificationAsync(user.Id, title, message, type, null, sentByUserId);
            }

            _logger.LogInformation("Notification sent to all users with role {Role}: {Title}", role, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to role {Role}", role);
        }
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
    {
        var query = _context.Notifications.AsQueryable()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .Include(n => n.SentBy)
            .Include(n => n.RelatedTicket)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        try
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification {NotificationId} marked as read by user {UserId}", notificationId, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        try
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("All notifications marked as read for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
        }
    }
}