using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inquiry.Common;
using Inquiry.Data;
using Inquiry.DTOs;
using Inquiry.Models;

namespace Inquiry.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Log in para sa Admin at regular Users (Renters)
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var normalizedEmail = dto.Email.Trim().ToLower();
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null || !PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Email or password is incorrect." });
        }

        // Suriin kung active ang account
        if (user.IsDeleted || user.Status == "Deleted")
        {
            return Unauthorized(new { message = "This account has been deleted. Please contact support." });
        }

        if (user.Status == "Locked")
        {
            return Unauthorized(new { message = "This account is locked. Please contact the administrator." });
        }

        if (user.Status == "Archived")
        {
            return Unauthorized(new { message = "This account is currently archived. Please contact support to restore it." });
        }

        // I-record ang huling active time sa database
        user.LastActiveAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Role 1 = Admin, Role 2 = Owner, Role 3 = Renter
        // Sa frontend, ang Admin/Owner portal ay gumagamit ng role: "owner"
        // Ang Renter portal ay gumagamit ng role: "renter"
        var role = (user.RoleId == 1 || user.RoleId == 2) ? "owner" : "renter";

        return Ok(new
        {
            role,
            user = new
            {
                id = user.UserId,
                roleId = user.RoleId,
                roleName = user.Role?.RoleName ?? (user.RoleId == 1 ? "Admin" : "Renter"),
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                phone = user.Phone,
                city = user.City,
                status = user.Status,
                isDeleted = user.IsDeleted,
                lastActiveAt = user.LastActiveAt,
                createdAt = user.CreatedAt
            }
        });
    }

    /// <summary>
    /// Create account para sa regular users (Renters)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var normalizedEmail = dto.Email.Trim().ToLower();
        var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (exists)
        {
            return Conflict(new { field = "email", message = "An account already uses this email. Log in instead." });
        }

        var renter = new User
        {
            RoleId = 3, // Role 3: Renter
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            PasswordHash = PasswordHelper.HashPassword(dto.Password),
            Status = "Active",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(renter);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            role = "renter",
            user = new
            {
                id = renter.UserId,
                roleId = renter.RoleId,
                roleName = "Renter",
                firstName = renter.FirstName,
                lastName = renter.LastName,
                email = renter.Email,
                phone = renter.Phone,
                city = renter.City,
                status = renter.Status,
                isDeleted = renter.IsDeleted,
                createdAt = renter.CreatedAt
            }
        });
    }

    /// <summary>
    /// Create account para sa Admin / Owner
    /// </summary>
    [HttpPost("create-admin")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Optional security check gamit ang ADMIN_SECRET_KEY mula sa .env
        var expectedKey = _configuration["ADMIN_SECRET_KEY"];
        if (!string.IsNullOrEmpty(expectedKey) && dto.AdminSecretKey != expectedKey)
        {
            return Unauthorized(new { message = "Invalid admin creation secret key." });
        }

        var normalizedEmail = dto.Email.Trim().ToLower();
        var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (exists)
        {
            return Conflict(new { field = "email", message = "An account already uses this email." });
        }

        var admin = new User
        {
            RoleId = 1, // Role 1: Admin
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone?.Trim(),
            City = dto.City?.Trim() ?? "Quezon City",
            PasswordHash = PasswordHelper.HashPassword(dto.Password),
            Status = "Active",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(admin);
        await _context.SaveChangesAsync();

        // Gawan din ng OwnerProfile ang admin para makapag-manage ng properties
        var profile = new OwnerProfile
        {
            UserId = admin.UserId,
            ResponseTimeId = 3 // "within a day"
        };
        await _context.OwnerProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            role = "owner",
            user = new
            {
                id = admin.UserId,
                roleId = admin.RoleId,
                roleName = "Admin",
                firstName = admin.FirstName,
                lastName = admin.LastName,
                email = admin.Email,
                phone = admin.Phone,
                city = admin.City,
                status = admin.Status
            }
        });
    }

    /// <summary>
    /// Logout endpoint
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logged out successfully." });
    }
}
