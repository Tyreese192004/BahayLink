using Inquiry.DTOs;

namespace Inquiry.Services;

public interface IPropertyService
{
    Task<IEnumerable<PropertyResponseDto>> GetAllPropertiesAsync(bool includeDeleted = false);
    Task<IEnumerable<PropertyResponseDto>> GetPropertiesByOwnerAsync(long ownerId);
    Task<PropertyResponseDto?> GetPropertyByIdAsync(long id);
    Task<PropertyResponseDto> CreatePropertyAsync(CreatePropertyDto dto);
    Task<PropertyResponseDto?> UpdatePropertyAsync(long id, CreatePropertyDto dto);
    Task<bool> SoftDeletePropertyAsync(long id);
    Task<bool> RestorePropertyAsync(long id);
    Task<bool> PermanentDeletePropertyAsync(long id);
}

