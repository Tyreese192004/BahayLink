using Microsoft.EntityFrameworkCore;
using Inquiry.Common;
using Inquiry.Data;
using Inquiry.DTOs;
using Inquiry.Models;
using Inquiry.Repositories;

namespace Inquiry.Services;

public class InquiryService : IInquiryService
{
    private readonly IInquiryRepository _inquiryRepository;
    private readonly ApplicationDbContext _context;

    public InquiryService(IInquiryRepository inquiryRepository, ApplicationDbContext context)
    {
        _inquiryRepository = inquiryRepository;
        _context = context;
    }

    public async Task<IEnumerable<InquiryResponseDto>> GetInquiriesAsync(long? ownerId = null, long? renterId = null)
    {
        var inquiries = await _inquiryRepository.GetInquiriesAsync(ownerId, renterId);
        return inquiries.Select(MapToResponseDto);
    }

    public async Task<InquiryResponseDto?> GetInquiryByIdAsync(long inquiryId)
    {
        var inquiry = await _inquiryRepository.GetInquiryByIdAsync(inquiryId);
        return inquiry == null ? null : MapToResponseDto(inquiry);
    }

    public async Task<InquiryResponseDto> CreateInquiryAsync(CreateInquiryDto dto)
    {
        // 1. Resolve renter ID
        long renterId = dto.RenterId ?? 0;
        if (renterId <= 0)
        {
            if (!string.IsNullOrWhiteSpace(dto.RenterEmail))
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.RenterEmail.ToLower());
                if (existingUser != null)
                {
                    renterId = existingUser.UserId;
                }
                else
                {
                    var names = (dto.RenterName ?? "Guest Renter").Trim().Split(' ', 2);
                    var newUser = new User
                    {
                        RoleId = 2, // Renter
                        FirstName = names[0],
                        LastName = names.Length > 1 ? names[1] : "",
                        Email = dto.RenterEmail.Trim(),
                        Phone = dto.RenterPhone?.Trim(),
                        PasswordHash = PasswordHelper.HashPassword("GuestPass123!"),
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Users.AddAsync(newUser);
                    await _context.SaveChangesAsync();
                    renterId = newUser.UserId;
                }
            }
            else
            {
                var firstRenter = await _context.Users.FirstOrDefaultAsync(u => u.RoleId == 2);
                renterId = firstRenter?.UserId ?? 1;
            }
        }

        // 2. Create Inquiry
        var inquiry = new Inquiry.Models.Inquiry
        {
            PropertyId = dto.PropertyId,
            RenterId = renterId,
            StatusId = 1, // Pending
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _inquiryRepository.CreateInquiryAsync(inquiry);

        // 3. Create initial message
        var initialMessage = new Message
        {
            InquiryId = created.InquiryId,
            SenderId = renterId,
            Body = dto.Message.Trim(),
            SentAt = DateTime.UtcNow
        };
        await _inquiryRepository.AddMessageAsync(initialMessage);

        // 4. Return fresh entity with relations
        var reloaded = await _inquiryRepository.GetInquiryByIdAsync(created.InquiryId);
        return MapToResponseDto(reloaded ?? created);
    }

    public async Task<MessageResponseDto> SendMessageAsync(SendMessageDto dto)
    {
        var inquiry = await _inquiryRepository.GetInquiryByIdAsync(dto.InquiryId);
        if (inquiry == null)
        {
            throw new InvalidOperationException($"Inquiry #{dto.InquiryId} not found.");
        }

        var message = new Message
        {
            InquiryId = dto.InquiryId,
            SenderId = dto.SenderId,
            Body = dto.Body.Trim(),
            SentAt = DateTime.UtcNow
        };

        await _inquiryRepository.AddMessageAsync(message);

        // If message is from owner and inquiry was Pending, set to Ongoing
        if (inquiry.Property != null && inquiry.Property.OwnerId == dto.SenderId && inquiry.StatusId == 1)
        {
            await _inquiryRepository.UpdateInquiryStatusAsync(dto.InquiryId, 2); // Ongoing
        }

        var sender = await _context.Users.FindAsync(dto.SenderId);
        return new MessageResponseDto
        {
            MessageId = message.MessageId,
            InquiryId = message.InquiryId,
            SenderId = message.SenderId,
            SenderName = sender != null ? $"{sender.FirstName} {sender.LastName}".Trim() : "User",
            SenderFirstName = sender?.FirstName ?? "User",
            SenderRole = sender?.RoleId == 1 ? "owner" : (sender?.RoleId == 3 ? "admin" : "renter"),
            Body = message.Body,
            SentAt = message.SentAt,
            ReadAt = message.ReadAt
        };
    }

    public async Task<bool> UpdateStatusAsync(long inquiryId, string statusName)
    {
        short statusId = statusName.ToLower() switch
        {
            "pending" => 1,
            "ongoing" => 2,
            "resolved" => 3,
            _ => 1
        };

        return await _inquiryRepository.UpdateInquiryStatusAsync(inquiryId, statusId);
    }

    private static InquiryResponseDto MapToResponseDto(Inquiry.Models.Inquiry i)
    {
        var prop = i.Property;
        var renter = i.Renter;
        var owner = prop?.Owner?.User;
        var coverPhoto = prop?.PropertyPhotos?.FirstOrDefault()?.PhotoUrl;

        return new InquiryResponseDto
        {
            InquiryId = i.InquiryId,
            PropertyId = i.PropertyId,
            PropertyName = prop?.Name ?? "Property Listing",
            PropertyLocation = prop?.Address ?? prop?.City ?? "Philippines",
            PropertyPrice = prop?.MonthlyPrice ?? 0,
            PropertyImage = coverPhoto,

            RenterId = i.RenterId,
            RenterName = renter != null ? $"{renter.FirstName} {renter.LastName}".Trim() : "Renter",
            RenterFirstName = renter?.FirstName ?? "Renter",
            RenterLastName = renter?.LastName ?? "",
            RenterEmail = renter?.Email ?? "",
            RenterPhone = renter?.Phone,

            OwnerId = prop?.OwnerId ?? 0,
            OwnerName = owner != null ? $"{owner.FirstName} {owner.LastName}".Trim() : "Property Owner",
            OwnerFirstName = owner?.FirstName ?? "Property Owner",
            OwnerLastName = owner?.LastName ?? "",
            OwnerEmail = owner?.Email ?? "",
            OwnerPhone = owner?.Phone,

            Status = i.Status?.StatusName ?? "Pending",
            StatusId = i.StatusId,
            CreatedAt = i.CreatedAt,

            Messages = (i.Messages ?? new List<Message>())
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageResponseDto
                {
                    MessageId = m.MessageId,
                    InquiryId = m.InquiryId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender != null ? $"{m.Sender.FirstName} {m.Sender.LastName}".Trim() : (m.SenderId == i.RenterId ? (renter?.FirstName ?? "Renter") : (owner?.FirstName ?? "Owner")),
                    SenderFirstName = m.Sender?.FirstName ?? (m.SenderId == i.RenterId ? (renter?.FirstName ?? "Renter") : (owner?.FirstName ?? "Owner")),
                    SenderRole = m.SenderId == i.RenterId ? "renter" : "owner",
                    Body = m.Body,
                    SentAt = m.SentAt,
                    ReadAt = m.ReadAt
                }).ToList()
        };
    }
}
