using Microsoft.EntityFrameworkCore;
using Inquiry.Data;
using Inquiry.Models;

namespace Inquiry.Repositories;

public class InquiryRepository : IInquiryRepository
{
    private readonly ApplicationDbContext _context;

    public InquiryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Inquiry.Models.Inquiry>> GetInquiriesAsync(long? ownerId = null, long? renterId = null, bool includeDeleted = false)
    {
        var query = _context.Inquiries
            .Include(i => i.Property)
                .ThenInclude(p => p!.PropertyPhotos)
            .Include(i => i.Property)
                .ThenInclude(p => p!.Owner)
                    .ThenInclude(o => o!.User)
            .Include(i => i.Renter)
            .Include(i => i.Status)
            .Include(i => i.Messages)
                .ThenInclude(m => m.Sender)
            .AsNoTracking()
            .AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(i => !i.IsDeleted);
        }

        if (ownerId.HasValue)
        {
            query = query.Where(i => i.Property != null && i.Property.OwnerId == ownerId.Value);
        }

        if (renterId.HasValue)
        {
            query = query.Where(i => i.RenterId == renterId.Value);
        }

        return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
    }

    public async Task<Inquiry.Models.Inquiry?> GetInquiryByIdAsync(long inquiryId)
    {
        return await _context.Inquiries
            .Include(i => i.Property)
                .ThenInclude(p => p!.PropertyPhotos)
            .Include(i => i.Property)
                .ThenInclude(p => p!.Owner)
                    .ThenInclude(o => o!.User)
            .Include(i => i.Renter)
            .Include(i => i.Status)
            .Include(i => i.Messages)
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(i => i.InquiryId == inquiryId && !i.IsDeleted);
    }

    public async Task<Inquiry.Models.Inquiry> CreateInquiryAsync(Inquiry.Models.Inquiry inquiry)
    {
        await _context.Inquiries.AddAsync(inquiry);
        await _context.SaveChangesAsync();
        return inquiry;
    }

    public async Task<bool> UpdateInquiryStatusAsync(long inquiryId, short statusId)
    {
        var inquiry = await _context.Inquiries.FindAsync(inquiryId);
        if (inquiry == null) return false;

        inquiry.StatusId = statusId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Message> AddMessageAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<IEnumerable<Message>> GetMessagesByInquiryIdAsync(long inquiryId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.InquiryId == inquiryId && !m.IsDeleted)
            .OrderBy(m => m.SentAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

