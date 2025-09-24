using System.ComponentModel.DataAnnotations;

namespace EnterpriseTicketing.Models;

public class ChatMessage
{
    public int Id { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsSystemMessage { get; set; } = false;

    // Foreign keys
    public int TicketId { get; set; }
    public int SenderId { get; set; }

    // Navigation properties
    public virtual Ticket Ticket { get; set; } = null!;
    public virtual User Sender { get; set; } = null!;
    public virtual ICollection<ChatAttachment> Attachments { get; set; } = new List<ChatAttachment>();
}