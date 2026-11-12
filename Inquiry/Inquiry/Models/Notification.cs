using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("notifications")]
public class Notification
{
    [Key]
    [Column("notification_id")]
    public long NotificationId { get; set; }

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

    [Required]
    [Column("notification_type_id")]
    public short NotificationTypeId { get; set; }

    [Column("inquiry_id")]
    public long? InquiryId { get; set; }

    [Column("message_id")]
    public long? MessageId { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false; // Temporary delete flag

    [Column("deleted_at", TypeName = "timestamp with time zone")]
    public DateTime? DeletedAt { get; set; }

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(NotificationTypeId))]
    public NotificationType? NotificationType { get; set; }

    [ForeignKey(nameof(InquiryId))]
    public Inquiry? Inquiry { get; set; }

    [ForeignKey(nameof(MessageId))]
    public Message? Message { get; set; }
}
