using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("messages")]
public class Message
{
    [Key]
    [Column("message_id")]
    public long MessageId { get; set; }

    [Required]
    [Column("inquiry_id")]
    public long InquiryId { get; set; }

    [Required]
    [Column("sender_id")]
    public long SenderId { get; set; }

    [Required]
    [Column("body", TypeName = "text")]
    public string Body { get; set; } = string.Empty;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false; // Temporary delete flag

    [Column("deleted_at", TypeName = "timestamp with time zone")]
    public DateTime? DeletedAt { get; set; }

    [Column("sent_at", TypeName = "timestamp with time zone")]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [Column("read_at", TypeName = "timestamp with time zone")]
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(InquiryId))]
    public Inquiry? Inquiry { get; set; }

    [ForeignKey(nameof(SenderId))]
    public User? Sender { get; set; }

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
