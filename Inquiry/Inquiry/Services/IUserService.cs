using Inquiry.DTOs;

namespace Inquiry.Services;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(string? tab = null);
    Task<UserResponseDto?> GetUserByIdAsync(long id);
    Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserResponseDto?> UpdateUserAsync(long id, UpdateUserDto updateUserDto);
    Task<bool> SoftDeleteUserAsync(long id);
    Task<bool> RestoreUserAsync(long id);
    Task<bool> UpdateUserStatusAsync(long id, string status);
    Task<bool> PermanentDeleteUserAsync(long id);
    Task<bool> RecordLastActiveAsync(long id);
}
