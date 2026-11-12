namespace Inquiry.DTOs;

public class UserResponseDto
{
    public long UserId { get; set; }
    public short RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? AvatarUrl { get; set; }
    public string Status { get; set; } = "Active"; // Active, Locked, Archived, Deleted
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
