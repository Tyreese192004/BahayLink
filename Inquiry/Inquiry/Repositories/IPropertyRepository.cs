using Inquiry.Models;

namespace Inquiry.Repositories;

public interface IPropertyRepository
{
    Task<IEnumerable<Property>> GetAllAsync(bool includeDeleted = false);
    Task<IEnumerable<Property>> GetByOwnerIdAsync(long ownerId);
    Task<Property?> GetByIdAsync(long id);
    Task<Property> CreateAsync(Property property);
    Task<Property> UpdateAsync(Property property);
    Task<bool> SoftDeleteAsync(long id);
    Task<bool> RestoreAsync(long id);
    Task<bool> PermanentDeleteAsync(long id);
}

