namespace Inquiry.DTOs;

public class PropertyResponseDto
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public short PropertyTypeId { get; set; }
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? City { get; set; }
    public int ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public decimal Price { get; set; }
    public short Bedrooms { get; set; }
    public short Bathrooms { get; set; }
    public decimal Area { get; set; }
    public bool Available { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Amenities { get; set; } = new();
    public List<string> Requirements { get; set; } = new();
    public List<string> Images { get; set; } = new();
}

