using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("user_id")]
    public long UserId { get; set; }

    [Required]
    [Column("role_id")]
    public short RoleId { get; set; }

    [Required]
    [MaxLength(60)]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [MaxLength(100)]
    [Column("city")]
    public string? City { get; set; }

    [Required]
    [Column("password_hash", TypeName = "text")]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Active"; // Active, Locked, Archived, Deleted

    [Column("avatar_url", TypeName = "text")]
    public string? AvatarUrl { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false; // Temporary delete flag

    [Column("deleted_at", TypeName = "timestamp with time zone")]
    public DateTime? DeletedAt { get; set; }

    [Column("last_active_at", TypeName = "timestamp with time zone")]
    public DateTime? LastActiveAt { get; set; }

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }

    public OwnerProfile? OwnerProfile { get; set; }

    [InverseProperty(nameof(Inquiry.Renter))]
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();

    [InverseProperty(nameof(Message.Sender))]
    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
