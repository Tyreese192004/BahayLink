-- =========================================================
-- BahayLink - Seed Admin Account & Default Master Data
-- =========================================================

-- 1. Insert Default Roles
INSERT INTO roles (role_id, role_name) VALUES
    (1, 'Admin'),
    (2, 'Owner'),
    (3, 'Renter')
ON CONFLICT (role_id) DO NOTHING;

-- 2. Insert Response Time Options
INSERT INTO response_time_options (response_time_id, label) VALUES
    (1, 'within an hour'),
    (2, 'within a few hours'),
    (3, 'within a day')
ON CONFLICT (response_time_id) DO NOTHING;

-- 3. Insert Inquiry Statuses
INSERT INTO inquiry_statuses (status_id, status_name) VALUES
    (1, 'Pending'),
    (2, 'Ongoing'),
    (3, 'Resolved')
ON CONFLICT (status_id) DO NOTHING;

-- 4. Insert Property Types
INSERT INTO property_types (property_type_id, type_name) VALUES
    (1, 'House'),
    (2, 'Apartment'),
    (3, 'Room')
ON CONFLICT (property_type_id) DO NOTHING;

-- 5. Insert Default Provinces
INSERT INTO provinces (province_id, province_name) VALUES
    (1, 'Tarlac'),
    (2, 'Pangasinan'),
    (3, 'Pampanga'),
    (4, 'Quezon Province'),
    (5, 'Laguna')
ON CONFLICT (province_id) DO NOTHING;

-- 6. Insert Admin User with Hashed Password
-- Password: "Admin123." (mula sa .env)
-- SHA-256 Base64 Hash: "9ANYM8uRGj6GNJA+SY/4SwXUtm9zxhVa8/WLIq01A/Q="
INSERT INTO users (role_id, first_name, last_name, email, phone, city, password_hash, created_at)
VALUES (
    1,
    'Maria',
    'Santos',
    'admin@bahaylink.ph',
    '09175550142',
    'Quezon City',
    'g/zT9xKbCB+usEPcByYuY/6lmdpL5oaaeheA9whKFbQ=',
    NOW()
)
ON CONFLICT (email) DO NOTHING;

-- 7. Insert Owner Profile for Admin
INSERT INTO owner_profiles (user_id, response_time_id)
SELECT user_id, 3
FROM users
WHERE email = 'admin@bahaylink.ph'
ON CONFLICT (user_id) DO NOTHING;

