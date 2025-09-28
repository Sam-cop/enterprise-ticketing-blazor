using System.ComponentModel.DataAnnotations;

namespace EnterpriseTicketing.Models;

public class Notification
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.Info;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    // Foreign keys
    public int UserId { get; set; }
    public int? RelatedTicketId { get; set; }
    public int? SentByUserId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual User? SentBy { get; set; }
    public virtual Ticket? RelatedTicket { get; set; }
}

public enum NotificationType
{
    Info = 0,
    Success = 1,
    Warning = 2,
    Error = 3,
    TicketAssigned = 4,
    TicketUpdated = 5,
    NewMessage = 6,
    AdminMessage = 7
}