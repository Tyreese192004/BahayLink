using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("response_time_options")]
public class ResponseTimeOption
{
    [Key]
    [Column("response_time_id")]
    public short ResponseTimeId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("label")]
    public string Label { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<OwnerProfile> OwnerProfiles { get; set; } = new List<OwnerProfile>();
}

