using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Inquiry.Models;

[PrimaryKey(nameof(PropertyId), nameof(AmenityId))]
[Table("property_amenities")]
public class PropertyAmenity
{
    [Column("property_id")]
    public long PropertyId { get; set; }

    [Column("amenity_id")]
    public int AmenityId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PropertyId))]
    public Property? Property { get; set; }

    [ForeignKey(nameof(AmenityId))]
    public Amenity? Amenity { get; set; }
}

