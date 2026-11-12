using Inquiry.Models;

namespace Inquiry.Repositories;

public interface IInquiryRepository
{
    Task<IEnumerable<Inquiry.Models.Inquiry>> GetInquiriesAsync(long? ownerId = null, long? renterId = null, bool includeDeleted = false);
    Task<Inquiry.Models.Inquiry?> GetInquiryByIdAsync(long inquiryId);
    Task<Inquiry.Models.Inquiry> CreateInquiryAsync(Inquiry.Models.Inquiry inquiry);
    Task<bool> UpdateInquiryStatusAsync(long inquiryId, short statusId);
    Task<Message> AddMessageAsync(Message message);
    Task<IEnumerable<Message>> GetMessagesByInquiryIdAsync(long inquiryId);
    Task SaveChangesAsync();
}

