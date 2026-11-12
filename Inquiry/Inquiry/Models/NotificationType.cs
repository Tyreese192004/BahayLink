using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("notification_types")]
public class NotificationType
{
    [Key]
    [Column("notification_type_id")]
    public short NotificationTypeId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("type_name")]
    public string TypeName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

