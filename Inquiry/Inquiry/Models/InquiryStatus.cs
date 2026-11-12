using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("inquiry_statuses")]
public class InquiryStatus
{
    [Key]
    [Column("status_id")]
    public short StatusId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("status_name")]
    public string StatusName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
}

