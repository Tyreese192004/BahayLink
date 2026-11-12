using System.ComponentModel.DataAnnotations;

namespace Inquiry.DTOs;

public class UpdateUserDto
{
    [MaxLength(60)]
    public string? FirstName { get; set; }

    [MaxLength(60)]
    public string? LastName { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    public string? AvatarUrl { get; set; }
}

public class UploadAvatarDto
{
    public string? AvatarUrl { get; set; }
}
