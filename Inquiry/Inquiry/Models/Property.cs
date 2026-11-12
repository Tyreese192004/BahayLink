using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("properties")]
public class Property
{
    [Key]
    [Column("property_id")]
    public long PropertyId { get; set; }

    [Required]
    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Required]
    [Column("property_type_id")]
    public short PropertyTypeId { get; set; }

    [Required]
    [Column("province_id")]
    public int ProvinceId { get; set; }

    [Required]
    [MaxLength(150)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1200)]
    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("address")]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("city")]
    public string? City { get; set; }

    [Column("monthly_price", TypeName = "numeric(12, 2)")]
    public decimal MonthlyPrice { get; set; }

    [Column("bedrooms")]
    public short Bedrooms { get; set; } = 1;

    [Column("bathrooms")]
    public short Bathrooms { get; set; } = 1;

    [Column("area", TypeName = "numeric(8, 2)")]
    public decimal Area { get; set; } = 0;

    [Column("is_available")]
    public bool IsAvailable { get; set; } = true;

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Active"; // Active, Archived, Deleted

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false; // Temporary delete flag

    [Column("deleted_at", TypeName = "timestamp with time zone")]
    public DateTime? DeletedAt { get; set; }

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(OwnerId))]
    public OwnerProfile? Owner { get; set; }

    [ForeignKey(nameof(PropertyTypeId))]
    public PropertyType? PropertyType { get; set; }

    [ForeignKey(nameof(ProvinceId))]
    public Province? Province { get; set; }

    public ICollection<PropertyAmenity> PropertyAmenities { get; set; } = new List<PropertyAmenity>();
    public ICollection<PropertyPhoto> PropertyPhotos { get; set; } = new List<PropertyPhoto>();
    public ICollection<PropertyRequirement> PropertyRequirements { get; set; } = new List<PropertyRequirement>();
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
}
