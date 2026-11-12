using Inquiry.Models;

namespace Inquiry.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync(string? tab = null);
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> SoftDeleteAsync(long id);
    Task<bool> RestoreAsync(long id);
    Task<bool> UpdateStatusAsync(long id, string status);
    Task<bool> PermanentDeleteAsync(long id);
    Task<bool> RecordLastActiveAsync(long id);
    Task<bool> ExistsAsync(long id);
}
