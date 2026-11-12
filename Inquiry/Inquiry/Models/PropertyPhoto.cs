using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("property_photos")]
public class PropertyPhoto
{
    [Key]
    [Column("photo_id")]
    public long PhotoId { get; set; }

    [Required]
    [Column("property_id")]
    public long PropertyId { get; set; }

    [Required]
    [Column("photo_url", TypeName = "text")]
    public string PhotoUrl { get; set; } = string.Empty;

    [Column("position")]
    public short Position { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PropertyId))]
    public Property? Property { get; set; }
}

