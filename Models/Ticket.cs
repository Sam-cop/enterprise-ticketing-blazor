using System.ComponentModel.DataAnnotations;

namespace EnterpriseTicketing.Models;

public class Ticket
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public TicketCategory Category { get; set; } = TicketCategory.General;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    // Foreign keys
    public int CreatedById { get; set; }
    public int? AssignedToId { get; set; }

    // Navigation properties
    public virtual User CreatedBy { get; set; } = null!;
    public virtual User? AssignedTo { get; set; }
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
}

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3,
    Reopened = 4
}

public enum TicketPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum TicketCategory
{
    General = 0,
    Hardware = 1,
    Software = 2,
    Network = 3,
    Security = 4,
    Account = 5
}