using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("provinces")]
public class Province
{
    [Key]
    [Column("province_id")]
    public int ProvinceId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("province_name")]
    public string ProvinceName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}

