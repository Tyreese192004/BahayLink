using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("inquiries")]
public class Inquiry
{
    [Key]
    [Column("inquiry_id")]
    public long InquiryId { get; set; }

    [Required]
    [Column("property_id")]
    public long PropertyId { get; set; }

    [Required]
    [Column("renter_id")]
    public long RenterId { get; set; }

    [Required]
    [Column("status_id")]
    public short StatusId { get; set; }

    [MaxLength(1000)]
    [Column("message")]
    public string? Message { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false; // Temporary delete flag

    [Column("deleted_at", TypeName = "timestamp with time zone")]
    public DateTime? DeletedAt { get; set; }

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(PropertyId))]
    public Property? Property { get; set; }

    [ForeignKey(nameof(RenterId))]
    public User? Renter { get; set; }

    [ForeignKey(nameof(StatusId))]
    public InquiryStatus? Status { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
