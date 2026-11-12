using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("property_requirements")]
public class PropertyRequirement
{
    [Key]
    [Column("requirement_id")]
    public long RequirementId { get; set; }

    [Required]
    [Column("property_id")]
    public long PropertyId { get; set; }

    [Column("line_no")]
    public short LineNo { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("requirement_text")]
    public string RequirementText { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey(nameof(PropertyId))]
    public Property? Property { get; set; }
}

