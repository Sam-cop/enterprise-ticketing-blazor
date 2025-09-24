using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using EnterpriseTicketing.Data;

namespace EnterpriseTicketing.Hubs;

public class NotificationHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ApplicationDbContext context, ILogger<NotificationHub> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task JoinUserGroup(string userEmail)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user != null)
            {
                var groupName = $"user_{user.Id}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation("User {UserEmail} joined notification group", userEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining user group for {UserEmail}", userEmail);
        }
    }

    public async Task LeaveUserGroup(string userEmail)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user != null)
            {
                var groupName = $"user_{user.Id}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation("User {UserEmail} left notification group", userEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving user group for {UserEmail}", userEmail);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User disconnected from notification hub: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}