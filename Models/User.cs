using System.ComponentModel.DataAnnotations;

namespace EnterpriseTicketing.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();
    public virtual ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}

public enum UserRole
{
    User = 0,
    Agent = 1,
    Admin = 2
}