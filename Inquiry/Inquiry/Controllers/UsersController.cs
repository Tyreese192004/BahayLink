using Microsoft.AspNetCore.Mvc;
using Inquiry.DTOs;
using Inquiry.Services;

namespace Inquiry.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    private readonly IWebHostEnvironment _env;

    public UsersController(IUserService userService, ILogger<UsersController> logger, IWebHostEnvironment env)
    {
        _userService = userService;
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// Kunin ang lahat ng users (maliban sa admin). Pwedeng i-filter via ?tab=active, ?tab=archive, o ?tab=deleted
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll([FromQuery] string? tab = null)
    {
        var users = await _userService.GetAllUsersAsync(tab);
        return Ok(users);
    }

    /// <summary>
    /// Kunin ang isang user gamit ang ID
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserResponseDto>> GetById(long id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = $"User with ID {id} not found." });
        }
        return Ok(user);
    }

    /// <summary>
    /// Lumikha ng bagong user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.UserId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "An error occurred while creating user." });
        }
    }

    /// <summary>
    /// I-update ang profile details ng user
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<UserResponseDto>> Update(long id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _userService.UpdateUserAsync(id, dto);
        if (updated == null)
        {
            return NotFound(new { message = $"User with ID {id} not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// I-update ang status ng user (Active, Locked, Archived, Deleted)
    /// </summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateUserStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _userService.UpdateUserStatusAsync(id, dto.Status);
        if (!success)
        {
            return NotFound(new { message = $"User with ID {id} not found or is protected." });
        }

        return Ok(new { message = $"User status updated to {dto.Status}.", status = dto.Status });
    }

    /// <summary>
    /// Temporary Delete (Soft delete) - inililipat ang user sa Recently Deleted
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> SoftDelete(long id)
    {
        var deleted = await _userService.SoftDeleteUserAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"User with ID {id} not found or is protected." });
        }

        return Ok(new { message = "User moved to Recently Deleted (soft-deleted)." });
    }

    /// <summary>
    /// Restore user - ibinabalik mula sa Recently Deleted o Archive papuntang Active
    /// </summary>
    [HttpPost("{id:long}/restore")]
    public async Task<IActionResult> Restore(long id)
    {
        var restored = await _userService.RestoreUserAsync(id);
        if (!restored)
        {
            return NotFound(new { message = $"User with ID {id} not found." });
        }

        return Ok(new { message = "User account successfully restored to Active." });
    }

    /// <summary>
    /// Permanent Delete - tuluyang binubura sa database (Hard delete)
    /// </summary>
    [HttpDelete("{id:long}/permanent")]
    public async Task<IActionResult> PermanentDelete(long id)
    {
        var deleted = await _userService.PermanentDeleteUserAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"User with ID {id} not found or is protected." });
        }

        return Ok(new { message = "User account permanently deleted from database." });
    }

    /// <summary>
    /// Mag-upload ng profile picture (Multipart Form-Data o JSON Body) na naka-save sa database
    /// </summary>
    [HttpPost("{id:long}/photo")]
    [Consumes("multipart/form-data", "application/json")]
    public async Task<IActionResult> UploadPhoto(long id, [FromForm] IFormFile? file, [FromBody] UploadAvatarDto? dto)
    {
        string? avatarUrl = null;

        // 1. Direct file upload
        if (file != null && file.Length > 0)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext))
            {
                return BadRequest(new { message = "Only image files (.jpg, .jpeg, .png, .webp, .gif) are allowed." });
            }

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsDir = Path.Combine(webRoot, "uploads", "avatars");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            var fileName = $"avatar_{id}_{DateTime.UtcNow.Ticks}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            avatarUrl = $"/uploads/avatars/{fileName}";
        }
        else if (dto != null && !string.IsNullOrWhiteSpace(dto.AvatarUrl))
        {
            avatarUrl = dto.AvatarUrl;
        }

        if (string.IsNullOrWhiteSpace(avatarUrl))
        {
            return BadRequest(new { message = "No valid image file or photo URL provided." });
        }

        var updated = await _userService.UpdateUserAsync(id, new UpdateUserDto { AvatarUrl = avatarUrl });
        if (updated == null)
        {
            return NotFound(new { message = $"User with ID {id} not found." });
        }

        return Ok(new { message = "Profile picture updated successfully.", avatarUrl });
    }

    /// <summary>
    /// Itala ang huling active time ng user
    /// </summary>
    [HttpPost("{id:long}/active")]
    public async Task<IActionResult> RecordActive(long id)
    {
        await _userService.RecordLastActiveAsync(id);
        return Ok(new { message = "Active timestamp recorded." });
    }
}
