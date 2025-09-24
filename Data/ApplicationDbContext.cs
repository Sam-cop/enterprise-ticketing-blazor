using Microsoft.EntityFrameworkCore;
using EnterpriseTicketing.Models;

namespace EnterpriseTicketing.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatAttachment> ChatAttachments { get; set; }
    public DbSet<TicketAttachment> TicketAttachments { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);

            // Manager-Client relationship
            entity.HasOne(e => e.Manager)
                .WithMany(u => u.Clients)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Ticket configuration
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(4000);

            // Relationships
            entity.HasOne(e => e.CreatedBy)
                .WithMany(u => u.CreatedTickets)
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedTo)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(e => e.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ChatMessage configuration
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).HasMaxLength(4000);

            // Relationships
            entity.HasOne(e => e.Ticket)
                .WithMany(t => t.ChatMessages)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sender)
                .WithMany(u => u.ChatMessages)
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ChatAttachment configuration
        modelBuilder.Entity<ChatAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(100);

            entity.HasOne(e => e.ChatMessage)
                .WithMany(cm => cm.Attachments)
                .HasForeignKey(e => e.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TicketAttachment configuration
        modelBuilder.Entity<TicketAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(100);

            entity.HasOne(e => e.Ticket)
                .WithMany(t => t.Attachments)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Message).HasMaxLength(4000);

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SentBy)
                .WithMany()
                .HasForeignKey(e => e.SentByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RelatedTicket)
                .WithMany()
                .HasForeignKey(e => e.RelatedTicketId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed admin user
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "admin@mail.com",
                FirstName = "System",
                LastName = "Administrator",
                Department = "IT",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );
    }
}