using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("amenities")]
public class Amenity
{
    [Key]
    [Column("amenity_id")]
    public int AmenityId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("amenity_name")]
    public string AmenityName { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("icon")]
    public string? Icon { get; set; }

    [Column("is_custom")]
    public bool IsCustom { get; set; } = false;

    // Navigation properties
    public ICollection<PropertyAmenity> PropertyAmenities { get; set; } = new List<PropertyAmenity>();
}

