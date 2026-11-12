using Microsoft.EntityFrameworkCore;
using Inquiry.Data;
using Inquiry.Models;

namespace Inquiry.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync(string? tab = null)
    {
        // Exclude Admin accounts (RoleId == 1) from general user list
        var query = _context.Users
            .Include(u => u.Role)
            .Where(u => u.RoleId != 1);

        if (!string.IsNullOrWhiteSpace(tab))
        {
            var cleanTab = tab.Trim().ToLowerInvariant();
            if (cleanTab == "active")
            {
                query = query.Where(u => !u.IsDeleted && u.Status == "Active");
            }
            else if (cleanTab == "archive" || cleanTab == "archived")
            {
                query = query.Where(u => !u.IsDeleted && (u.Status == "Locked" || u.Status == "Archived"));
            }
            else if (cleanTab == "deleted")
            {
                query = query.Where(u => u.IsDeleted || u.Status == "Deleted");
            }
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User> CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> SoftDeleteAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.RoleId == 1) return false;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.Status = "Deleted";

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsDeleted = false;
        user.DeletedAt = null;
        user.Status = "Active";

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(long id, string status)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.RoleId == 1) return false;

        user.Status = status;
        if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
        {
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
        }
        else
        {
            user.IsDeleted = false;
            user.DeletedAt = null;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PermanentDeleteAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.RoleId == 1) return false;

        // Hard / permanent delete
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RecordLastActiveAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.LastActiveAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Users.AnyAsync(u => u.UserId == id);
    }
}
