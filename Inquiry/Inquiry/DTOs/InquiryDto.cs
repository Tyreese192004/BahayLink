using System.ComponentModel.DataAnnotations;

namespace Inquiry.DTOs;

public class InquiryResponseDto
{
    public long InquiryId { get; set; }
    public long PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string PropertyLocation { get; set; } = string.Empty;
    public decimal PropertyPrice { get; set; }
    public string? PropertyImage { get; set; }

    public long RenterId { get; set; }
    public string RenterName { get; set; } = string.Empty;
    public string RenterFirstName { get; set; } = string.Empty;
    public string RenterLastName { get; set; } = string.Empty;
    public string RenterEmail { get; set; } = string.Empty;
    public string? RenterPhone { get; set; }

    public long OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerFirstName { get; set; } = string.Empty;
    public string OwnerLastName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string? OwnerPhone { get; set; }

    public string Status { get; set; } = "Pending";
    public short StatusId { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<MessageResponseDto> Messages { get; set; } = new();
}

public class MessageResponseDto
{
    public long MessageId { get; set; }
    public long InquiryId { get; set; }
    public long SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderFirstName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty; // "renter", "owner", "admin"
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class CreateInquiryDto
{
    [Required]
    public long PropertyId { get; set; }

    public long? RenterId { get; set; }

    public string? RenterName { get; set; }

    [EmailAddress]
    public string? RenterEmail { get; set; }

    public string? RenterPhone { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}

public class SendMessageDto
{
    [Required]
    public long InquiryId { get; set; }

    [Required]
    public long SenderId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Body { get; set; } = string.Empty;
}

public class UpdateInquiryStatusDto
{
    [Required]
    public string Status { get; set; } = "Pending";
}

