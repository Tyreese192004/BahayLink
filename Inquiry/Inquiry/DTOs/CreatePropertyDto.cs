using System.ComponentModel.DataAnnotations;

namespace Inquiry.DTOs;

public class CreatePropertyDto
{
    public long? OwnerId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Type { get; set; } // "House", "Apartment", "Room"
    public short? PropertyTypeId { get; set; }

    [MaxLength(1200)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(255)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? City { get; set; }

    public int? ProvinceId { get; set; }

    [Range(0, 10000000)]
    public decimal Price { get; set; }

    [Range(0, 50)]
    public short Bedrooms { get; set; } = 1;

    [Range(1, 50)]
    public short Bathrooms { get; set; } = 1;

    [Range(0, 100000)]
    public decimal Area { get; set; } = 0;

    public bool Available { get; set; } = true;

    public List<string> Amenities { get; set; } = new();
    public List<string> Requirements { get; set; } = new();
    public List<string> Images { get; set; } = new();
}

