using System.ComponentModel.DataAnnotations;

namespace Inquiry.DTOs;

public class UpdateUserStatusDto
{
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Locked, Archived, Deleted
}

