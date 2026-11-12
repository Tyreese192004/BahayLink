using Inquiry.Common;
using Inquiry.DTOs;
using Inquiry.Models;
using Inquiry.Repositories;

namespace Inquiry.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(string? tab = null)
    {
        var users = await _userRepository.GetAllAsync(tab);
        return users.Select(MapToResponseDto);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(long id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToResponseDto(user);
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var existingEmail = await _userRepository.GetByEmailAsync(createUserDto.Email);
        if (existingEmail != null)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var user = new User
        {
            RoleId = createUserDto.RoleId,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Email = createUserDto.Email,
            Phone = createUserDto.Phone,
            City = createUserDto.City,
            Status = "Active",
            IsDeleted = false,
            PasswordHash = PasswordHelper.HashPassword(createUserDto.Password),
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user);
        return MapToResponseDto(createdUser);
    }

    public async Task<UserResponseDto?> UpdateUserAsync(long id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName))
        {
            user.FirstName = updateUserDto.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.LastName))
        {
            user.LastName = updateUserDto.LastName;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
        {
            user.Email = updateUserDto.Email;
        }

        if (updateUserDto.Phone != null)
        {
            user.Phone = updateUserDto.Phone;
        }

        if (updateUserDto.City != null)
        {
            user.City = updateUserDto.City;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.AvatarUrl))
        {
            user.AvatarUrl = updateUserDto.AvatarUrl;
        }

        var updatedUser = await _userRepository.UpdateAsync(user);
        return MapToResponseDto(updatedUser);
    }

    public async Task<bool> SoftDeleteUserAsync(long id)
    {
        return await _userRepository.SoftDeleteAsync(id);
    }

    public async Task<bool> RestoreUserAsync(long id)
    {
        return await _userRepository.RestoreAsync(id);
    }

    public async Task<bool> UpdateUserStatusAsync(long id, string status)
    {
        return await _userRepository.UpdateStatusAsync(id, status);
    }

    public async Task<bool> PermanentDeleteUserAsync(long id)
    {
        return await _userRepository.PermanentDeleteAsync(id);
    }

    public async Task<bool> RecordLastActiveAsync(long id)
    {
        return await _userRepository.RecordLastActiveAsync(id);
    }

    private static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto
        {
            UserId = user.UserId,
            RoleId = user.RoleId,
            RoleName = user.Role?.RoleName ?? (user.RoleId == 1 ? "Admin" : user.RoleId == 2 ? "Owner" : "Renter"),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            City = user.City,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status ?? "Active",
            IsDeleted = user.IsDeleted,
            DeletedAt = user.DeletedAt,
            LastActiveAt = user.LastActiveAt,
            CreatedAt = user.CreatedAt
        };
    }
}
