-- =============================================
-- TaskScheduler Database Creation Script
-- PostgreSQL Version
-- =============================================

-- Drop database if exists (for fresh installation)
DROP DATABASE IF EXISTS task_scheduler_db;

-- Create database
CREATE DATABASE task_scheduler_db
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

COMMENT ON DATABASE task_scheduler_db IS 'TaskScheduler Application Database';

-- Connect to the database
\c task_scheduler_db;

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- For full-text search on tasks

-- Display success message
SELECT 'Database task_scheduler_db created successfully!' as status;
