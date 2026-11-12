using Microsoft.EntityFrameworkCore;
using Inquiry.Data;
using Inquiry.DTOs;
using Inquiry.Models;
using Inquiry.Repositories;

namespace Inquiry.Services;

public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly ApplicationDbContext _context;

    public PropertyService(IPropertyRepository propertyRepository, ApplicationDbContext context)
    {
        _propertyRepository = propertyRepository;
        _context = context;
    }

    public async Task<IEnumerable<PropertyResponseDto>> GetAllPropertiesAsync(bool includeDeleted = false)
    {
        var list = await _propertyRepository.GetAllAsync(includeDeleted);
        return list.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<PropertyResponseDto>> GetPropertiesByOwnerAsync(long ownerId)
    {
        var list = await _propertyRepository.GetByOwnerIdAsync(ownerId);
        return list.Select(MapToResponseDto);
    }

    public async Task<PropertyResponseDto?> GetPropertyByIdAsync(long id)
    {
        var prop = await _propertyRepository.GetByIdAsync(id);
        return prop == null ? null : MapToResponseDto(prop);
    }

    public async Task<PropertyResponseDto> CreatePropertyAsync(CreatePropertyDto dto)
    {
        // 1. Resolve OwnerId: If not specified, get first Admin/Owner
        long ownerId = dto.OwnerId ?? 0;
        if (ownerId <= 0)
        {
            var defaultOwner = await _context.OwnerProfiles.FirstOrDefaultAsync();
            if (defaultOwner != null)
            {
                ownerId = defaultOwner.UserId;
            }
            else
            {
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.RoleId == 1 || u.RoleId == 2);
                if (admin != null)
                {
                    ownerId = admin.UserId;
                }
                else
                {
                    var anyUser = await _context.Users.FirstOrDefaultAsync();
                    if (anyUser != null)
                    {
                        ownerId = anyUser.UserId;
                    }
                    else
                    {
                        // Fallback: seed admin user
                        var adminUser = new User
                        {
                            RoleId = 1,
                            FirstName = "Maria",
                            LastName = "Santos",
                            Email = "admin@bahaylink.ph",
                            PasswordHash = Common.PasswordHelper.HashPassword("Admin123."),
                            Status = "Active",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.Users.AddAsync(adminUser);
                        await _context.SaveChangesAsync();
                        ownerId = adminUser.UserId;
                    }
                }
            }
        }

        // Ensure OwnerProfile exists for this user
        var ownerProfile = await _context.OwnerProfiles.FindAsync(ownerId);
        if (ownerProfile == null && ownerId > 0)
        {
            ownerProfile = new OwnerProfile
            {
                UserId = ownerId,
                ResponseTimeId = 3 // default "within a day"
            };
            await _context.OwnerProfiles.AddAsync(ownerProfile);
            await _context.SaveChangesAsync();
        }

        // 2. Resolve PropertyType nang ligtas
        short typeId = 1;
        if (!string.IsNullOrWhiteSpace(dto.Type))
        {
            var trimmedType = dto.Type.Trim();
            var matchedType = await _context.PropertyTypes
                .FirstOrDefaultAsync(t => t.TypeName.ToLower() == trimmedType.ToLower() ||
                                          t.TypeName.ToLower().Contains(trimmedType.ToLower()) ||
                                          trimmedType.ToLower().Contains(t.TypeName.ToLower()));
            if (matchedType != null)
            {
                typeId = matchedType.PropertyTypeId;
            }
            else
            {
                var firstType = await _context.PropertyTypes.FirstOrDefaultAsync();
                if (firstType != null) typeId = firstType.PropertyTypeId;
            }
        }
        else
        {
            var firstType = await _context.PropertyTypes.FirstOrDefaultAsync();
            if (firstType != null) typeId = firstType.PropertyTypeId;
        }

        // 3. Resolve Province nang ligtas (hindi magko-cause ng unique constraint violation)
        int provinceId = 1;
        if (dto.ProvinceId.HasValue && dto.ProvinceId.Value > 0)
        {
            var exists = await _context.Provinces.AnyAsync(p => p.ProvinceId == dto.ProvinceId.Value);
            if (exists)
            {
                provinceId = dto.ProvinceId.Value;
            }
            else
            {
                var firstProvince = await _context.Provinces.FirstOrDefaultAsync();
                if (firstProvince != null) provinceId = firstProvince.ProvinceId;
            }
        }
        else if (!string.IsNullOrWhiteSpace(dto.City))
        {
            var trimmedCity = dto.City.Trim();
            var matchedProvince = await _context.Provinces
                .FirstOrDefaultAsync(p => p.ProvinceName.ToLower() == trimmedCity.ToLower() ||
                                          p.ProvinceName.ToLower().Contains(trimmedCity.ToLower()) ||
                                          trimmedCity.ToLower().Contains(p.ProvinceName.ToLower()));
            if (matchedProvince != null)
            {
                provinceId = matchedProvince.ProvinceId;
            }
            else
            {
                var firstProvince = await _context.Provinces.FirstOrDefaultAsync();
                if (firstProvince != null) provinceId = firstProvince.ProvinceId;
            }
        }
        else
        {
            var firstProvince = await _context.Provinces.FirstOrDefaultAsync();
            if (firstProvince != null) provinceId = firstProvince.ProvinceId;
        }

        // 4. Construct Property Entity
        var property = new Property
        {
            OwnerId = ownerId,
            PropertyTypeId = typeId,
            ProvinceId = provinceId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Address = dto.Location.Trim(),
            City = dto.City?.Trim() ?? "Tarlac",
            MonthlyPrice = dto.Price,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            Area = dto.Area,
            IsAvailable = dto.Available,
            Status = "Active",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        // Add Photos
        if (dto.Images != null && dto.Images.Count > 0)
        {
            short pos = 1;
            foreach (var img in dto.Images)
            {
                if (!string.IsNullOrWhiteSpace(img))
                {
                    property.PropertyPhotos.Add(new PropertyPhoto
                    {
                        PhotoUrl = img,
                        Position = pos++
                    });
                }
            }
        }

        // Add Requirements
        if (dto.Requirements != null && dto.Requirements.Count > 0)
        {
            short lineNo = 1;
            foreach (var req in dto.Requirements)
            {
                if (!string.IsNullOrWhiteSpace(req))
                {
                    property.PropertyRequirements.Add(new PropertyRequirement
                    {
                        LineNo = lineNo++,
                        RequirementText = req.Trim()
                    });
                }
            }
        }

        // Add Amenities
        if (dto.Amenities != null && dto.Amenities.Count > 0)
        {
            foreach (var amenName in dto.Amenities.Distinct())
            {
                if (string.IsNullOrWhiteSpace(amenName)) continue;
                var trimmed = amenName.Trim();

                try
                {
                    var amenity = await _context.Amenities.FirstOrDefaultAsync(a => a.AmenityName.ToLower() == trimmed.ToLower());
                    if (amenity == null)
                    {
                        amenity = new Amenity
                        {
                            AmenityName = trimmed,
                            IsCustom = true
                        };
                        await _context.Amenities.AddAsync(amenity);
                        await _context.SaveChangesAsync();
                    }

                    property.PropertyAmenities.Add(new PropertyAmenity
                    {
                        AmenityId = amenity.AmenityId
                    });
                }
                catch
                {
                    // Ligtas laban sa sequence issue sa amenities
                }
            }
        }

        var created = await _propertyRepository.CreateAsync(property);

        // Re-query with includes for complete response
        var result = await _propertyRepository.GetByIdAsync(created.PropertyId);
        return MapToResponseDto(result ?? created);
    }

    public async Task<PropertyResponseDto?> UpdatePropertyAsync(long id, CreatePropertyDto dto)
    {
        var existing = await _propertyRepository.GetByIdAsync(id);
        if (existing == null) return null;

        // Update basic fields
        existing.Name = dto.Name.Trim();
        existing.Description = dto.Description?.Trim();
        existing.Address = dto.Location.Trim();
        if (!string.IsNullOrWhiteSpace(dto.City)) existing.City = dto.City.Trim();
        existing.MonthlyPrice = dto.Price;
        existing.Bedrooms = dto.Bedrooms;
        existing.Bathrooms = dto.Bathrooms;
        existing.Area = dto.Area;
        existing.IsAvailable = dto.Available;

        // Resolve PropertyType by name if not provided by ID
        if (dto.PropertyTypeId.HasValue)
        {
            existing.PropertyTypeId = dto.PropertyTypeId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(dto.Type))
        {
            var trimmedType = dto.Type.Trim();
            var matchedType = await _context.PropertyTypes
                .FirstOrDefaultAsync(t => t.TypeName.ToLower() == trimmedType.ToLower() ||
                                          t.TypeName.ToLower().Contains(trimmedType.ToLower()) ||
                                          trimmedType.ToLower().Contains(t.TypeName.ToLower()));
            if (matchedType != null)
            {
                existing.PropertyTypeId = matchedType.PropertyTypeId;
            }
        }

        // Resolve Province by city name if not provided by ID
        if (dto.ProvinceId.HasValue)
        {
            existing.ProvinceId = dto.ProvinceId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(dto.City))
        {
            var trimmedCity = dto.City.Trim();
            var matchedProvince = await _context.Provinces
                .FirstOrDefaultAsync(p => p.ProvinceName.ToLower() == trimmedCity.ToLower() ||
                                          p.ProvinceName.ToLower().Contains(trimmedCity.ToLower()) ||
                                          trimmedCity.ToLower().Contains(p.ProvinceName.ToLower()));
            if (matchedProvince != null)
            {
                existing.ProvinceId = matchedProvince.ProvinceId;
            }
        }

        // Update Photos: remove old, add new
        if (dto.Images != null)
        {
            var oldPhotos = existing.PropertyPhotos.ToList();
            _context.PropertyPhotos.RemoveRange(oldPhotos);

            short pos = 1;
            foreach (var img in dto.Images)
            {
                if (!string.IsNullOrWhiteSpace(img))
                {
                    existing.PropertyPhotos.Add(new PropertyPhoto
                    {
                        PropertyId = existing.PropertyId,
                        PhotoUrl = img,
                        Position = pos++
                    });
                }
            }
        }

        // Update Requirements: remove old, add new
        if (dto.Requirements != null)
        {
            var oldReqs = existing.PropertyRequirements.ToList();
            _context.PropertyRequirements.RemoveRange(oldReqs);

            short lineNo = 1;
            foreach (var req in dto.Requirements)
            {
                if (!string.IsNullOrWhiteSpace(req))
                {
                    existing.PropertyRequirements.Add(new PropertyRequirement
                    {
                        PropertyId = existing.PropertyId,
                        LineNo = lineNo++,
                        RequirementText = req.Trim()
                    });
                }
            }
        }

        // Update Amenities: remove old, add new
        if (dto.Amenities != null)
        {
            var oldAmenities = existing.PropertyAmenities.ToList();
            _context.PropertyAmenities.RemoveRange(oldAmenities);

            foreach (var amenName in dto.Amenities.Distinct())
            {
                if (string.IsNullOrWhiteSpace(amenName)) continue;
                var trimmed = amenName.Trim();

                try
                {
                    var amenity = await _context.Amenities
                        .FirstOrDefaultAsync(a => a.AmenityName.ToLower() == trimmed.ToLower());
                    if (amenity == null)
                    {
                        amenity = new Amenity
                        {
                            AmenityName = trimmed,
                            IsCustom = true
                        };
                        await _context.Amenities.AddAsync(amenity);
                        await _context.SaveChangesAsync();
                    }

                    existing.PropertyAmenities.Add(new PropertyAmenity
                    {
                        PropertyId = existing.PropertyId,
                        AmenityId = amenity.AmenityId
                    });
                }
                catch
                {
                    // Safe against sequence/duplicate issues
                }
            }
        }

        var updated = await _propertyRepository.UpdateAsync(existing);

        // Re-query with includes for complete response
        var result = await _propertyRepository.GetByIdAsync(updated.PropertyId);
        return MapToResponseDto(result ?? updated);
    }

    public async Task<bool> SoftDeletePropertyAsync(long id)
    {
        return await _propertyRepository.SoftDeleteAsync(id);
    }

    public async Task<bool> RestorePropertyAsync(long id)
    {
        return await _propertyRepository.RestoreAsync(id);
    }

    public async Task<bool> PermanentDeletePropertyAsync(long id)
    {
        return await _propertyRepository.PermanentDeleteAsync(id);
    }

    private static PropertyResponseDto MapToResponseDto(Property p)
    {
        return new PropertyResponseDto
        {
            Id = p.PropertyId,
            OwnerId = p.OwnerId,
            Name = p.Name,
            Type = p.PropertyType?.TypeName ?? "House",
            PropertyTypeId = p.PropertyTypeId,
            Description = p.Description,
            Location = p.Address,
            City = p.City ?? p.Province?.ProvinceName,
            ProvinceId = p.ProvinceId,
            ProvinceName = p.Province?.ProvinceName,
            Price = p.MonthlyPrice,
            Bedrooms = p.Bedrooms,
            Bathrooms = p.Bathrooms,
            Area = p.Area,
            Available = p.IsAvailable,
            Status = p.Status ?? "Active",
            IsDeleted = p.IsDeleted,
            CreatedAt = p.CreatedAt,
            Amenities = p.PropertyAmenities?.Select(pa => pa.Amenity?.AmenityName ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>(),
            Requirements = p.PropertyRequirements?.OrderBy(r => r.LineNo).Select(r => r.RequirementText).ToList() ?? new List<string>(),
            Images = p.PropertyPhotos?.OrderBy(ph => ph.Position).Select(ph => ph.PhotoUrl).ToList() ?? new List<string>()
        };
    }
}

