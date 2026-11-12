-- ========================================================
-- BahayLink Schema Migration Script
-- Run this in pgAdmin or psql if you want to apply manually
-- ========================================================

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

