using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("property_types")]
public class PropertyType
{
    [Key]
    [Column("property_type_id")]
    public short PropertyTypeId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("type_name")]
    public string TypeName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}

