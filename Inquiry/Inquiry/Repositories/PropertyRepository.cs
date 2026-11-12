using Microsoft.EntityFrameworkCore;
using Inquiry.Data;
using Inquiry.Models;

namespace Inquiry.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly ApplicationDbContext _context;

    public PropertyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Property>> GetAllAsync(bool includeDeleted = false)
    {
        var query = _context.Properties
            .Include(p => p.PropertyType)
            .Include(p => p.Province)
            .Include(p => p.PropertyPhotos)
            .Include(p => p.PropertyRequirements)
            .Include(p => p.PropertyAmenities)
                .ThenInclude(pa => pa.Amenity)
            .AsNoTracking();

        if (!includeDeleted)
        {
            query = query.Where(p => !p.IsDeleted);
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Property>> GetByOwnerIdAsync(long ownerId)
    {
        return await _context.Properties
            .Include(p => p.PropertyType)
            .Include(p => p.Province)
            .Include(p => p.PropertyPhotos)
            .Include(p => p.PropertyRequirements)
            .Include(p => p.PropertyAmenities)
                .ThenInclude(pa => pa.Amenity)
            .Where(p => p.OwnerId == ownerId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Property?> GetByIdAsync(long id)
    {
        return await _context.Properties
            .Include(p => p.PropertyType)
            .Include(p => p.Province)
            .Include(p => p.PropertyPhotos)
            .Include(p => p.PropertyRequirements)
            .Include(p => p.PropertyAmenities)
                .ThenInclude(pa => pa.Amenity)
            .FirstOrDefaultAsync(p => p.PropertyId == id);
    }

    public async Task<Property> CreateAsync(Property property)
    {
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<Property> UpdateAsync(Property property)
    {
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<bool> SoftDeleteAsync(long id)
    {
        var prop = await _context.Properties.FindAsync(id);
        if (prop == null) return false;

        prop.IsDeleted = true;
        prop.DeletedAt = DateTime.UtcNow;
        prop.Status = "Deleted";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(long id)
    {
        var prop = await _context.Properties.FindAsync(id);
        if (prop == null) return false;

        prop.IsDeleted = false;
        prop.DeletedAt = null;
        prop.Status = "Active";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PermanentDeleteAsync(long id)
    {
        var prop = await _context.Properties.FindAsync(id);
        if (prop == null) return false;

        _context.Properties.Remove(prop);
        await _context.SaveChangesAsync();
        return true;
    }
}

