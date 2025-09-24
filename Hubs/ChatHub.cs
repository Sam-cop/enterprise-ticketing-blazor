using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using EnterpriseTicketing.Data;
using EnterpriseTicketing.Models;
using EnterpriseTicketing.Services;

namespace EnterpriseTicketing.Hubs;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChatHub> _logger;
    private readonly INotificationService _notificationService;

    public ChatHub(ApplicationDbContext context, ILogger<ChatHub> logger, INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task JoinTicketGroup(int ticketId, string userEmail)
    {
        var groupName = $"ticket_{ticketId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserEmail} joined ticket group {TicketId}", userEmail, ticketId);
    }

    public async Task LeaveTicketGroup(int ticketId, string userEmail)
    {
        var groupName = $"ticket_{ticketId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserEmail} left ticket group {TicketId}", userEmail, ticketId);
    }

    public async Task SendMessageToTicket(int ticketId, string message, string senderEmail)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == senderEmail);
            if (user == null)
            {
                _logger.LogWarning("User not found: {SenderEmail}", senderEmail);
                return;
            }

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
            {
                _logger.LogWarning("Ticket not found: {TicketId}", ticketId);
                return;
            }

            var chatMessage = new ChatMessage
            {
                Message = message,
                TicketId = ticketId,
                SenderId = user.Id,
                SentAt = DateTime.UtcNow,
                IsSystemMessage = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Update ticket's UpdatedAt timestamp
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var groupName = $"ticket_{ticketId}";
            await Clients.Group(groupName).SendAsync("ReceiveMessage", new
            {
                Id = chatMessage.Id,
                Message = chatMessage.Message,
                SentAt = chatMessage.SentAt,
                SenderName = $"{user.FirstName} {user.LastName}",
                SenderEmail = user.Email,
                IsSystemMessage = chatMessage.IsSystemMessage
            });

            // Send notification to ticket participants (except sender)
            var ticketParticipants = new List<int>();
            if (ticket.CreatedById != user.Id)
                ticketParticipants.Add(ticket.CreatedById);
            if (ticket.AssignedToId.HasValue && ticket.AssignedToId.Value != user.Id)
                ticketParticipants.Add(ticket.AssignedToId.Value);

            foreach (var participantId in ticketParticipants.Distinct())
            {
                await _notificationService.SendNotificationAsync(
                    participantId,
                    "پیام جدید در تیکت",
                    $"پیام جدید در تیکت #{ticket.Id}: {ticket.Title}",
                    NotificationType.NewMessage,
                    ticket.Id,
                    user.Id
                );
            }

            _logger.LogInformation("Message sent to ticket {TicketId} by {SenderEmail}", ticketId, senderEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to ticket {TicketId}", ticketId);
        }
    }

    public async Task SendFileNotification(int ticketId, string fileName, string senderEmail, long fileSize)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == senderEmail);
            if (user == null)
            {
                _logger.LogWarning("User not found: {SenderEmail}", senderEmail);
                return;
            }

            var groupName = $"ticket_{ticketId}";
            await Clients.Group(groupName).SendAsync("ReceiveFileNotification", new
            {
                FileName = fileName,
                FileSize = fileSize,
                SenderName = $"{user.FirstName} {user.LastName}",
                SenderEmail = user.Email,
                UploadedAt = DateTime.UtcNow
            });

            _logger.LogInformation("File notification sent to ticket {TicketId} by {SenderEmail}: {FileName}", 
                ticketId, senderEmail, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending file notification to ticket {TicketId}", ticketId);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}