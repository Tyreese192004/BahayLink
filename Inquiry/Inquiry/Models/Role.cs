using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inquiry.Models;

[Table("roles")]
public class Role
{
    [Key]
    [Column("role_id")]
    public short RoleId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
}

