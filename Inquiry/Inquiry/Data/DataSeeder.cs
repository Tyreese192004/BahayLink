using Microsoft.EntityFrameworkCore;
using Inquiry.Common;
using Inquiry.Models;

namespace Inquiry.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");

        try
        {
            // Awtomatikong likhain ang database at tables kung wala pa
            await context.Database.EnsureCreatedAsync();

            // Awtomatikong idagdag ang mga bagong columns kung nag-e-exist na ang tables sa PostgreSQL
            var alterTablesSql = @"
                -- Properties table columns
                ALTER TABLE properties ADD COLUMN IF NOT EXISTS bedrooms SMALLINT DEFAULT 1;
                ALTER TABLE properties ADD COLUMN IF NOT EXISTS bathrooms SMALLINT DEFAULT 1;
                ALTER TABLE properties ADD COLUMN IF NOT EXISTS area NUMERIC(8, 2) DEFAULT 0;
                ALTER TABLE properties ADD COLUMN IF NOT EXISTS city VARCHAR(100);
                ALTER TABLE properties ADD COLUMN IF NOT EXISTS status VARCHAR(20) DEFAULT 'Active';
                ALTER TABLE properties ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN DEFAULT FALSE;
                ALTER TABLE properties ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;

                -- Users table columns
                ALTER TABLE users ADD COLUMN IF NOT EXISTS status VARCHAR(20) DEFAULT 'Active';
                ALTER TABLE users ADD COLUMN IF NOT EXISTS avatar_url TEXT;
                ALTER TABLE users ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN DEFAULT FALSE;
                ALTER TABLE users ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;
                ALTER TABLE users ADD COLUMN IF NOT EXISTS last_active_at TIMESTAMP WITH TIME ZONE;

                -- Inquiries table columns
                ALTER TABLE inquiries ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN DEFAULT FALSE;
                ALTER TABLE inquiries ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;

                -- Messages table columns
                ALTER TABLE messages ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN DEFAULT FALSE;
                ALTER TABLE messages ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;

                -- Notifications table columns
                ALTER TABLE notifications ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN DEFAULT FALSE;
                ALTER TABLE notifications ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;
            ";
            await context.Database.ExecuteSqlRawAsync(alterTablesSql);

            // 1. Seed Roles kung wala pa
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new() { RoleId = 1, RoleName = "Admin" },
                    new() { RoleId = 2, RoleName = "Owner" },
                    new() { RoleId = 3, RoleName = "Renter" }
                };
                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
                logger.LogInformation("Default roles (Admin, Owner, Renter) seeded successfully.");
            }

            // 2. Seed Response Time Options
            if (!await context.ResponseTimeOptions.AnyAsync())
            {
                var responseOptions = new List<ResponseTimeOption>
                {
                    new() { ResponseTimeId = 1, Label = "within an hour" },
                    new() { ResponseTimeId = 2, Label = "within a few hours" },
                    new() { ResponseTimeId = 3, Label = "within a day" }
                };
                await context.ResponseTimeOptions.AddRangeAsync(responseOptions);
                await context.SaveChangesAsync();
            }

            // 3. Seed Inquiry Statuses
            if (!await context.InquiryStatuses.AnyAsync())
            {
                var statuses = new List<InquiryStatus>
                {
                    new() { StatusId = 1, StatusName = "Pending" },
                    new() { StatusId = 2, StatusName = "Ongoing" },
                    new() { StatusId = 3, StatusName = "Resolved" }
                };
                await context.InquiryStatuses.AddRangeAsync(statuses);
                await context.SaveChangesAsync();
            }

            // 4. Seed Property Types
            if (!await context.PropertyTypes.AnyAsync())
            {
                var propTypes = new List<PropertyType>
                {
                    new() { PropertyTypeId = 1, TypeName = "House" },
                    new() { PropertyTypeId = 2, TypeName = "Apartment" },
                    new() { PropertyTypeId = 3, TypeName = "Room" }
                };
                await context.PropertyTypes.AddRangeAsync(propTypes);
                await context.SaveChangesAsync();
            }

            // 5. Seed Provinces
            if (!await context.Provinces.AnyAsync())
            {
                var provinces = new List<Province>
                {
                    new() { ProvinceId = 1, ProvinceName = "Tarlac" },
                    new() { ProvinceId = 2, ProvinceName = "Pangasinan" },
                    new() { ProvinceId = 3, ProvinceName = "Pampanga" },
                    new() { ProvinceId = 4, ProvinceName = "Quezon Province" },
                    new() { ProvinceId = 5, ProvinceName = "Laguna" }
                };
                await context.Provinces.AddRangeAsync(provinces);
                await context.SaveChangesAsync();
            }

            // 6. Seed Admin Account mula sa .env Configuration
            var adminEmail = configuration["ADMIN_EMAIL"] ?? "admin@bahaylink.ph";
            var adminPassword = configuration["ADMIN_PASSWORD"] ?? "Admin123.";
            var adminFirstName = configuration["ADMIN_FIRST_NAME"] ?? "Maria";
            var adminLastName = configuration["ADMIN_LAST_NAME"] ?? "Santos";
            var adminPhone = configuration["ADMIN_PHONE"] ?? "09175550142";
            var adminCity = configuration["ADMIN_CITY"] ?? "Quezon City";

            var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == adminEmail.ToLower());
            if (existingAdmin == null)
            {
                // I-hash ang password mula sa .env gamit ang secure SHA-256
                var hashedPassword = PasswordHelper.HashPassword(adminPassword);

                var adminUser = new User
                {
                    RoleId = 1, // Role 1: Admin
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    Email = adminEmail,
                    Phone = adminPhone,
                    City = adminCity,
                    PasswordHash = hashedPassword,
                    Status = "Active",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();

                // I-link ang OwnerProfile para sa Admin
                var ownerProfile = new OwnerProfile
                {
                    UserId = adminUser.UserId,
                    ResponseTimeId = 3 // "within a day"
                };
                await context.OwnerProfiles.AddAsync(ownerProfile);
                await context.SaveChangesAsync();

                logger.LogInformation("Admin account seeded successfully for {Email} with secure hashed password.", adminEmail);
            }

            // 7. Seed Sample Users para masubukan agad ang Active, Archive at Recently Deleted
            if (!await context.Users.AnyAsync(u => u.RoleId == 3))
            {
                var sampleRenters = new List<User>
                {
                    new()
                    {
                        RoleId = 3,
                        FirstName = "Juan",
                        LastName = "Dela Cruz",
                        Email = "juan@example.com",
                        Phone = "09181234567",
                        City = "Manila",
                        PasswordHash = PasswordHelper.HashPassword("Renter123!"),
                        Status = "Active",
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        LastActiveAt = DateTime.UtcNow.AddHours(-2)
                    },
                    new()
                    {
                        RoleId = 3,
                        FirstName = "Ana",
                        LastName = "Reyes",
                        Email = "ana@example.com",
                        Phone = "09192345678",
                        City = "Quezon City",
                        PasswordHash = PasswordHelper.HashPassword("Renter123!"),
                        Status = "Archived",
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-20),
                        LastActiveAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new()
                    {
                        RoleId = 3,
                        FirstName = "Pedro",
                        LastName = "Penduko",
                        Email = "pedro@example.com",
                        Phone = "09201122334",
                        City = "Tarlac City",
                        PasswordHash = PasswordHelper.HashPassword("Renter123!"),
                        Status = "Deleted",
                        IsDeleted = true,
                        DeletedAt = DateTime.UtcNow.AddDays(-2),
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        LastActiveAt = DateTime.UtcNow.AddDays(-3)
                    }
                };
                await context.Users.AddRangeAsync(sampleRenters);
                await context.SaveChangesAsync();
                logger.LogInformation("Sample users seeded for Active, Archive, and Recently Deleted.");
            }

            // 8. I-resync ang PostgreSQL sequences upang maiwasan ang duplicate key errors
            var resyncSequencesSql = @"
                DO $$
                BEGIN
                    BEGIN
                        PERFORM setval(pg_get_serial_sequence('roles', 'role_id'), COALESCE((SELECT MAX(role_id) FROM roles), 1));
                    EXCEPTION WHEN OTHERS THEN NULL;
                    END;
                    BEGIN
                        PERFORM setval(pg_get_serial_sequence('provinces', 'province_id'), COALESCE((SELECT MAX(province_id) FROM provinces), 1));
                    EXCEPTION WHEN OTHERS THEN NULL;
                    END;
                    BEGIN
                        PERFORM setval(pg_get_serial_sequence('property_types', 'property_type_id'), COALESCE((SELECT MAX(property_type_id) FROM property_types), 1));
                    EXCEPTION WHEN OTHERS THEN NULL;
                    END;
                    BEGIN
                        PERFORM setval(pg_get_serial_sequence('inquiry_statuses', 'status_id'), COALESCE((SELECT MAX(status_id) FROM inquiry_statuses), 1));
                    EXCEPTION WHEN OTHERS THEN NULL;
                    END;
                    BEGIN
                        PERFORM setval(pg_get_serial_sequence('response_time_options', 'response_time_id'), COALESCE((SELECT MAX(response_time_id) FROM response_time_options), 1));
                    EXCEPTION WHEN OTHERS THEN NULL;
                    END;
                    BEGIN
                        PERFORM setval(pg_get_serial_sequence('users', 'user_id'), COALESCE((SELECT MAX(user_id) FROM users), 1));
                    EXCEPTION WHEN OTHERS THEN NULL;
                    END;
                    BEGIN
                        PERFORM setval(pg_get_serial_sequence('properties', 'property_id'), COALESCE((SELECT MAX(property_id) FROM properties), 1));
                    EXCEPTION WHEN OTHERS THEN NULL;
                    END;
                    BEGIN
                        PERFORM setval(pg_get_serial_sequence('amenities', 'amenity_id'), COALESCE((SELECT MAX(amenity_id) FROM amenities), 1));
                    EXCEPTION WHEN OTHERS THEN NULL;
                    END;
                END $$;
            ";
            await context.Database.ExecuteSqlRawAsync(resyncSequencesSql);
        }
        catch (Exception ex)
        {
            logger.LogWarning("DataSeeder note: {Message}", ex.Message);
        }
    }
}
