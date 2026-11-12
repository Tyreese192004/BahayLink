using Inquiry.DTOs;

namespace Inquiry.Services;

public interface IInquiryService
{
    Task<IEnumerable<InquiryResponseDto>> GetInquiriesAsync(long? ownerId = null, long? renterId = null);
    Task<InquiryResponseDto?> GetInquiryByIdAsync(long inquiryId);
    Task<InquiryResponseDto> CreateInquiryAsync(CreateInquiryDto dto);
    Task<MessageResponseDto> SendMessageAsync(SendMessageDto dto);
    Task<bool> UpdateStatusAsync(long inquiryId, string statusName);
}

