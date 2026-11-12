using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("owner_profiles")]
public class OwnerProfile
{
    [Key]
    [Column("user_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long UserId { get; set; }

    [Column("response_time_id")]
    public short? ResponseTimeId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(ResponseTimeId))]
    public ResponseTimeOption? ResponseTimeOption { get; set; }

    [InverseProperty(nameof(Property.Owner))]
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}

