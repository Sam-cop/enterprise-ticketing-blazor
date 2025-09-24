using System.ComponentModel.DataAnnotations;

namespace EnterpriseTicketing.Models;

public class ChatAttachment
{
    public int Id { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Foreign key
    public int ChatMessageId { get; set; }

    // Navigation property
    public virtual ChatMessage ChatMessage { get; set; } = null!;
}